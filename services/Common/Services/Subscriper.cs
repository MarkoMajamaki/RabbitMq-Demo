using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common
{
    public interface ISubscriber
    {
        void Queue(Func<object, bool> callback, string queue,  ushort prefetchSize = 10);
        void Direct(Func<object, bool> callback, string queue, string exchange, string routingKey, ushort prefetchSize = 10, int timeToLive = 30000);
        void Topic(Func<object, bool> callback, string queue, string exchange, string routingKey, ushort prefetchSize = 10, int timeToLive = 30000);
        void Headers(Func<object, bool> callback, string queue, string exchange, Dictionary<string, object> header, bool shouldAllHeadersMatch = false, ushort prefetchSize = 10, int timeToLive = 30000);
        void Fanout(Func<object, bool> callback, string queue, string exchange, ushort prefetchSize = 10, int timeToLive = 30000);
        void Rpc(Func<object, object> callback, string queue);
    }

    /// <summary>
    /// Subscribe message from RabbitMQ
    /// </summary>
    public class Subscriber : ISubscriber
    {
        private IConnectionProvider _connectionProvider;
        private readonly ILogger _logger;

        public Subscriber(
            IConnectionProvider connectionProvider,
            ILogger<Publisher> logger)
        {
            _connectionProvider = connectionProvider;            
            _logger = logger;   
        }

        /// <summary>
        /// Subscribe queue with default exchange without routing key
        /// </summary>
       public void Queue(
            Func<object, bool> callback,
            string queue, 
            ushort prefetchSize = 10)
        {
            SubscribeInternal(
                callback: callback,
                queue: queue,
                prefetchSize: prefetchSize
            );
        }

        /// <summary>
        /// Subscribe message which has same exchange and routing key
        /// </summary>
        public void Direct(
            Func<object, bool> callback,
            string queue,
            string exchange,
            string routingKey,
            ushort prefetchSize = 10,
            int timeToLive = 30000)
        {
            SubscribeInternal(
                callback: callback,
                queue: queue,
                exchange: exchange,
                exchangeType: ExchangeType.Direct,
                routingKey: routingKey,
                prefetchSize: prefetchSize,
                timeToLive: timeToLive);
        }

        /// <summary>
        /// Subscribe message which has same exchange and routing key. Topic exchange also
        /// support wildcard matching (* and #) in routing key.
        /// </summary>
        public void Topic(
            Func<object, bool> callback,
            string queue,
            string exchange,
            string routingKey,
            ushort prefetchSize = 10,
            int timeToLive = 30000)
        {
            SubscribeInternal(
                callback: callback,
                queue: queue,
                exchange: exchange,
                exchangeType: ExchangeType.Topic,
                routingKey: routingKey,
                prefetchSize: prefetchSize,
                timeToLive: timeToLive);
        }

        /// <summary>
        /// Subscribe message which has same exchange and all or any headers. 
        /// </summary>
        public void Headers(
            Func<object, bool> callback,
            string queue,
            string exchange,
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
            
            SubscribeInternal(
                callback: callback,
                queue: queue,
                exchange: exchange,
                exchangeType: ExchangeType.Headers,
                header: actualHeader,
                prefetchSize: prefetchSize,
                timeToLive: timeToLive);
        }

        /// <summary>
        /// Subscribe message which are bound to exchange ignoring routing keys
        /// </summary>
        public void Fanout(
            Func<object, bool> callback,
            string queue,
            string exchange,
            ushort prefetchSize = 10,
            int timeToLive = 30000)
        {
            SubscribeInternal(
                callback: callback,
                queue: queue,
                exchange: exchange,
                exchangeType: ExchangeType.Fanout,
                prefetchSize: prefetchSize,
                timeToLive: timeToLive);
        }

        /// <summary>
        /// Subcriber for rpc call
        /// </summary>
        public void Rpc(
            Func<object, object> callback,
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

                var message = Encoding.UTF8.GetString(body);
                object response = null;

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

        /// <summary>
        /// Subscribe RabbitMq message
        /// </summary>
        private void SubscribeInternal(
            Func<string, bool> callback,
            string queue, 
            string exchange = null, 
            string exchangeType = null,
            string routingKey = null,
            ushort prefetchSize = 10,
            Dictionary<string, object> header = null,
            int timeToLive = 30000)
        {
            IConnection connection = _connectionProvider.Connect();

            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            if (exchange != null)
            {
                channel.ExchangeDeclare(
                    exchange: exchange,
                    type: exchangeType,
                    arguments: new Dictionary<string, object>
                    {
                        {"x-message-ttl", timeToLive }
                    }
                );

                channel.QueueBind(queue, exchange, routingKey ?? string.Empty, header);
            }

            channel.BasicQos(0, prefetchSize, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                bool success = callback(message);

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
                queue: queue,
                autoAck: false,
                consumer: consumer);
        }
    }
}