using System;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging
{
    public interface IQueueConsumer
    {
        void Subscribe(string queueName, Action<object> received);
    }

    public class QueueConsumer : IQueueConsumer
    {
        private IRabbitMqConnection _rabbitMqConnection;
        private readonly ILogger _logger;
        private EventingBasicConsumer _consumer;

        public QueueConsumer(
            IRabbitMqConnection rabbitMqConnection,
            ILogger<QueueConsumer> logger)
        {
            _rabbitMqConnection = rabbitMqConnection;            
            _logger = logger;
        }

        public void Subscribe(string queueName, Action<object> received)
        {
            IConnection connection = _rabbitMqConnection.Connect();

            var channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            
            _consumer = new EventingBasicConsumer(channel);

            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                received(message);

                _logger.LogDebug($"QueueConsumer.Subscribe: {message}");
            };

            channel.BasicConsume(
                queue: queueName,
                autoAck: true,
                consumer: _consumer);
        }
    }
}