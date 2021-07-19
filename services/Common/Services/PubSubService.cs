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
    public interface IPubSubService
    {
        /// <summary>
        /// Publish message to all queues which has same exchange and all or any headers depending listenere settings. 
        /// </summary>
        void Publish(object message, Dictionary<string, object> headers, int timeToLive = 30000);

        /// <summary>
        /// Subscribe message which has same exchange and all or any headers. 
        /// </summary>
        void Subscribe<TRequest>(Func<TRequest, Task<bool>> callback, Dictionary<string, object> header, bool shouldAllHeadersMatch = false, ushort prefetchSize = 10, int timeToLive = 30000);
    }

    /// <summary>
    /// Publish and subscribe messages via RabbitMQ
    /// </summary>
    public class PubSubService : IPubSubService
    {
        private const string _exchange = "default_exchange";
        private const string _queue = "default_queue";
        private IConnectionProvider _connectionProvider;
        private readonly ILogger _logger;

        public PubSubService(
            IConnectionProvider connectionProvider,
            ILogger<PubSubService> logger)
        {
            _connectionProvider = connectionProvider;            
            _logger = logger;   
        }

        /// <summary>
        /// Publish message to all queues which has same exchange and all or any headers depending listenere settings. 
        /// </summary>
        public void Publish(
            object message,
            Dictionary<string, object> headers, 
            int timeToLive = 30000)
        {
            // Get RabbitMQ connection
            IConnection connection = _connectionProvider.Connect();

            using(var channel = connection.CreateModel())            
            {
                channel.ExchangeDeclare(
                    exchange: _exchange,
                    type: ExchangeType.Headers,
                    arguments: new Dictionary<string, object>
                    {
                        {"x-message-ttl", timeToLive }
                    }
                );

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.Headers = headers;
                properties.Expiration = timeToLive.ToString();

                channel.BasicPublish(
                    exchange: _exchange,
                    routingKey: string.Empty,
                    basicProperties: properties,
                    body: body);
            }
        }

        /// <summary>
        /// Subscribe message which has same exchange and all or any headers. 
        /// </summary>
        public void Subscribe<TRequest>(
            Func<TRequest, Task<bool>> callback,
            Dictionary<string, object> header,
            bool shouldAllHeadersMatch = false,
            ushort prefetchSize = 10,
            int timeToLive = 30000)            
        {
            Dictionary<string, object> actualHeader = new Dictionary<string, object>(header);
            if (shouldAllHeadersMatch)
            {
                actualHeader.Add("x-match", "all");
            }
            else
            {
                actualHeader.Add("x-match", "any");
            }
            
            IConnection connection = _connectionProvider.Connect();

            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: _queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            channel.ExchangeDeclare(
                exchange: _exchange,
                type: ExchangeType.Headers,
                arguments: new Dictionary<string, object>
                {
                    {"x-message-ttl", timeToLive }
                }
            );

            channel.QueueBind(_queue, _exchange, string.Empty, header);

            channel.BasicQos(0, prefetchSize, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                string jsonMessage = Encoding.UTF8.GetString(body);
                TRequest message = JsonConvert.DeserializeObject<TRequest>(jsonMessage);
                bool success = await callback(message);

                if (success)
                {
                    channel.BasicAck(e.DeliveryTag, true);
                    _logger.LogDebug("Subscribe successed!");
                }
                else
                {
                    _logger.LogDebug("Subscribe failed!");
                }
            };

            channel.BasicConsume(
                queue: _queue,
                autoAck: false,
                consumer: consumer);
        }
    }
}