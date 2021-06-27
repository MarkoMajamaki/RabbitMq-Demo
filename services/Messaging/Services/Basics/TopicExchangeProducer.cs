using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Messaging
{
    public interface ITopicExchangeProducer
    {
        void Publish(object message, string exchangeName, string routingKey);
    }

    public class TopicExchangeProducer : ITopicExchangeProducer
    {
        private IRabbitMqConnection _rabbitMqConnection;
        private readonly ILogger _logger;

        public TopicExchangeProducer(
            IRabbitMqConnection rabbitMqConnection,
            ILogger<QueueProducer> logger)
        {
            _rabbitMqConnection = rabbitMqConnection;            
            _logger = logger;
        }

        public void Publish(object message, string exchangeName, string routingKey)
        {
            IConnection connection = _rabbitMqConnection.Connect();

            using(var channel = connection.CreateModel())            
            {
                channel.ExchangeDeclare(
                    exchange: exchangeName,
                    type: ExchangeType.Topic);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    basicProperties: null,
                    body: body);

                _logger.LogDebug($"TopicExchangeProducer.Publish: {message}");
            }
        }
    }
}