using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Messaging
{
    public interface IFanoutExchangeProducer
    {
        void Publish(object message, string exchangeName, string routingKey);
    }

    public class FanoutExchangeProducer : IFanoutExchangeProducer
    {
        private IRabbitMqConnection _rabbitMqConnection;
        private readonly ILogger _logger;

        public FanoutExchangeProducer(
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
                    type: ExchangeType.Fanout);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    basicProperties: null,
                    body: body);

                _logger.LogDebug($"FanoutExchangeProducer.Publish: {message}");
            }
        }
    }
}