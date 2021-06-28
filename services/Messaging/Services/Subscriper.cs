using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging
{
    public interface ISubscriber
    {
        void Queue(Func<string, bool> callback, string queue,  ushort prefetchSize = 10);
        void Direct(Func<string, bool> callback, string queue, string exchange, string routingKey, ushort prefetchSize = 10, int timeToLive = 30000);
        void Topic(Func<string, bool> callback, string queue, string exchange, string routingKey, ushort prefetchSize = 10, int timeToLive = 30000);
        void Headers(Func<string, bool> callback, string queue, string exchange, Dictionary<string, object> header, ushort prefetchSize = 10, int timeToLive = 30000);
        void Fanout(Func<string, bool> callback, string queue, string exchange, ushort prefetchSize = 10, int timeToLive = 30000);
    }

    /// <summary>
    /// Subscribe message from RabbitMq
    /// </summary>
    public class Subscriber : ISubscriber
    {
        private IConnectionProvider _connectionProvider;
        private readonly ILogger _logger;
        private EventingBasicConsumer _consumer;

        public Subscriber(
            IConnectionProvider connectionProvider,
            ILogger<Publisher> logger)
        {
            _connectionProvider = connectionProvider;            
            _logger = logger;   
        }

        /// <summary>
        /// Subscrible queue
        /// </summary>
        public void Queue(
            Func<string, bool> callback,
            string queue, 
            ushort prefetchSize = 10)
        {
            SubscribeInternal(
                callback: callback,
                queue: queue,
                prefetchSize: prefetchSize
            );
        }

        public void Direct(
            Func<string, bool> callback,
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

        public void Topic(
            Func<string, bool> callback,
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

        public void Headers(
            Func<string, bool> callback,
            string queue,
            string exchange,
            Dictionary<string, object> header,
            ushort prefetchSize = 10,
            int timeToLive = 30000)
        {
            SubscribeInternal(
                callback: callback,
                queue: queue,
                exchange: exchange,
                exchangeType: ExchangeType.Headers,
                header: header,
                prefetchSize: prefetchSize,
                timeToLive: timeToLive);
        }

        public void Fanout(
            Func<string, bool> callback,
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
        /// Internal implementation
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

            var model = connection.CreateModel();

            if (exchange != null)
            {
                model.ExchangeDeclare(
                    exchange: exchange,
                    type: exchangeType,
                    arguments: new Dictionary<string, object>
                    {
                        {"x-message-ttl", timeToLive }
                    }
                );
            }

            model.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            model.QueueBind(queue, exchange, routingKey ?? string.Empty, header);
            model.BasicQos(0, prefetchSize, false);

            _consumer = new EventingBasicConsumer(model);

            _consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                bool success = callback(message);

                if (success)
                {
                    model.BasicAck(e.DeliveryTag, true);
                    _logger.LogDebug("Subscribe successed!");
                }
                else
                {
                    _logger.LogDebug("Subscribe failed!");
                }

            };

            model.BasicConsume(
                queue: queue,
                autoAck: false,
                consumer: _consumer);
        }
    }
}