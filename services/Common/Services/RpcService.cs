using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common
{
    /// <summary>
    /// Invoke and handle RPC call via RabbitMq
    /// </summary>
    public interface IRpcService
    {
        /// <summary>
        /// /// Invoke RPC call and wait response
        /// </summary>
        Task<TResponse> Invoke<TResponse>(object message, string queue);

        /// <summary>
        /// Handle RPC call and return response
        /// </summary>
        void Handle<TRequest, TResponse>(Func<TRequest, TResponse> callback, string queue);
    }

    /// <summary>
    /// Invoke and handle RPC call via RabbitMq
    /// </summary>
    public class RpcService : IRpcService
    {
        private const string _exchange = "default_exchange";
        private const string _queue = "default_queue";
        private IConnectionProvider _connectionProvider;
        private readonly ILogger _logger;

        public RpcService(
            IConnectionProvider connectionProvider,
            ILogger<PubSubService> logger)
        {
            _connectionProvider = connectionProvider;            
            _logger = logger;   
        }

        /// <summary>
        /// Invoke RPC call and wait response
        /// </summary>
        public async Task<TResponse> Invoke<TResponse>(object message, string queue)
        {
            // Get RabbitMQ connection
            IConnection connection = _connectionProvider.Connect();
            var channel = connection.CreateModel();
            
            IBasicProperties props = channel.CreateBasicProperties();

            // Generate random correlation id
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;

            // Set response queue name
            string replyQueueName = channel.QueueDeclare().QueueName;
            props.ReplyTo = replyQueueName;

            // Response message queue
            TaskCompletionSource<TResponse> tcs = new TaskCompletionSource<TResponse>();

            // Listen message received to response queue
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();                
                string jsonResponse = Encoding.UTF8.GetString(body);
                TResponse response = JsonConvert.DeserializeObject<TResponse>(jsonResponse);

                // Add  to response queue if correlation id match
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    tcs.SetResult(response);
                }
                else
                {
                    tcs.SetCanceled();
                }

                // Close channel
                channel.Close();
            };

            // Start listening reply queue
            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);
                
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            // Send message
            channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: props,
                body: body);

            return await tcs.Task;
        }

        /// <summary>
        /// Handle RPC call and return response
        /// </summary>
        public void Handle<TRequest, TResponse>(
            Func<TRequest, TResponse> callback,
            string queue)
        {
            // Get RabbitMQ connection
            IConnection connection = _connectionProvider.Connect();
            var channel = connection.CreateModel();

            // Decleare queue to receive message
            channel.QueueDeclare(
                queue: queue, 
                durable: false,
                exclusive: false, 
                autoDelete: false, 
                arguments: null);

            channel.BasicQos(0, 1, false);

            // Listen queue to receive message
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(
                queue: queue,
                autoAck: false, 
                consumer: consumer);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;

                // Create reply props with correlation id from message received
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                string jsonMessage = Encoding.UTF8.GetString(body);
                TRequest message = JsonConvert.DeserializeObject<TRequest>(jsonMessage);

                var response = default(TResponse);

                try
                {
                    response = callback(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Message {message} in queue {queue} receive callback execution failed! " + e.Message);
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));

                    // Send reply message
                    channel.BasicPublish(
                        exchange: "", 
                        routingKey: props.ReplyTo,
                        basicProperties: replyProps, 
                        body: responseBytes);

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false);
                }
            };
        }
    }
}