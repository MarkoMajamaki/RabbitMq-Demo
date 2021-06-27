using System;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging
{
    public interface IFanoutExchangeConsumer
    {
        void Subscribe(string exchangeName, string queueName, string routingKey, Action<object> received);
    }

    public class FanoutExchangeConsumer : IFanoutExchangeConsumer
    {
        private IRabbitMqConnection _rabbitMqConnection;
        private readonly ILogger _logger;
        private EventingBasicConsumer _consumer;

        public FanoutExchangeConsumer(
            IRabbitMqConnection rabbitMqConnection,
            ILogger<QueueConsumer> logger)
        {
            _rabbitMqConnection = rabbitMqConnection;            
            _logger = logger;
        }

        public void Subscribe(string exchangeName, string queueName, string routingKey, Action<object> received)
        {
            IConnection connection = _rabbitMqConnection.Connect();

            var channel = connection.CreateModel();
        
            channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Fanout
            );

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            channel.QueueBind(queueName, exchangeName, routingKey);
            
            _consumer = new EventingBasicConsumer(channel);

            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                received(message);

                _logger.LogDebug($"FanoutExchangeConsumer.Subscribe: {message}");
            };

            channel.BasicConsume(
                queue: queueName,
                autoAck: true,
                consumer: _consumer);
        }
    }
}