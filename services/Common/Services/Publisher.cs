using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Common
{
    public interface IPublisher
    {
        void Queue(object message, string queue, int timeToLive = 30000);
        void Direct(object message, string exchange, string routingKey, int timeToLive = 30000);
        void Topic(object message, string exchange, string routingKey, int timeToLive = 30000);
        void Header(object message, string exchange, IDictionary<string, object> headers, int timeToLive = 30000);
        void Fanout(object message, string exchange, string routingKey, IDictionary<string, object> headers, int timeToLive = 30000);
    }

    /// <summary>
    /// Publish message to RabbitMQ
    /// </summary>
    public class Publisher : IPublisher
    {
        private IConnectionProvider _connectionProvider;
        private readonly ILogger _logger;

        public Publisher(
            IConnectionProvider connectionProvider,
            ILogger<Publisher> logger)
        {
            _connectionProvider = connectionProvider;            
            _logger = logger;   
        }

        /// <summary>
        /// Publish message to queue with default exchange
        /// </summary>
        public void Queue(
            object message,
            string queue,
            int timeToLive = 30000)
        {
            if (string.IsNullOrEmpty(queue))
            {
                throw new ArgumentException("Queue not set!");
            }

            PublishInternal(
                message: message,
                routingKey: queue,
                timeToLive: timeToLive);
        }

        /// <summary>
        /// Publish message to single queue which has same exchange and routing key
        /// </summary>
        public void Direct(
            object message,
            string exchange,
            string routingKey,
            int timeToLive = 30000)
        {
            if (string.IsNullOrEmpty(exchange))
            {
                throw new ArgumentException("Exchange not set!");
            }

            PublishInternal(
                message: message,
                exchange: exchange,
                exchangeType: ExchangeType.Direct,
                routingKey: routingKey,
                timeToLive: timeToLive);
        }

        /// <summary>
        /// Publish message to all queues which has same exchange and routing key. Topic exchange also
        /// support wildcard matching (* and #) in routing key.
        /// </summary>
        public void Topic(
            object message,
            string exchange,
            string routingKey,
            int timeToLive = 30000)
        {
            PublishInternal(
                message: message,
                exchange: exchange,
                exchangeType: ExchangeType.Topic,
                routingKey: routingKey,
                timeToLive: timeToLive);
        }

        /// <summary>
        /// Publish message to all queues which has same exchange and all or any headers. 
        /// </summary>
        public void Header(
            object message,
            string exchange,
            IDictionary<string, object> headers, 
            int timeToLive = 30000)
        {
            PublishInternal(
                message: message,
                exchange: exchange,
                exchangeType: ExchangeType.Headers,
                headers: headers,
                timeToLive: timeToLive);
        }

        /// <summary>
        /// Publish message to all queues which are bound to exchange ignoring routing keys
        /// </summary>
        public void Fanout(
            object message,
            string exchange,
            string routingKey,
            IDictionary<string, object> headers, 
            int timeToLive = 30000)
        {
            PublishInternal(
                message: message,
                exchange: exchange,
                exchangeType: ExchangeType.Fanout,
                routingKey: routingKey,
                headers: headers,
                timeToLive: timeToLive);
        }

        /// <summary>
        /// Publish RabbitMq message
        /// </summary>
        private void PublishInternal(
            object message,
            string queue = null,
            string exchange = null, 
            string exchangeType = null, 
            string routingKey = null, 
            IDictionary<string, object> headers = null, 
            int timeToLive = 30000)
        {
            // Get RabbitMQ connection
            IConnection connection = _connectionProvider.Connect();

            using(var model = connection.CreateModel())            
            {
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
                else 
                {
                    model.QueueDeclare(
                        routingKey,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                }

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                
                var properties = model.CreateBasicProperties();
                properties.Persistent = true;
                properties.Headers = headers;
                properties.Expiration = timeToLive.ToString();

                model.BasicPublish(
                    exchange: exchange ?? string.Empty,
                    routingKey: routingKey ?? string.Empty,
                    basicProperties: properties,
                    body: body);

                _logger.LogDebug($"DirectExchangeProducer.Publish: {message}");
            }
        }
    }
}