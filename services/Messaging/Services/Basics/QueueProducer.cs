using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Messaging
{
    public interface IQueueProducer
    {
        void Publish(object message, string queueName);
    }

    public class QueueProducer : IQueueProducer
    {
        private IRabbitMqConnection _rabbitMqConnection;
        private readonly ILogger _logger;

        public QueueProducer(
            IRabbitMqConnection rabbitMqConnection,
            ILogger<QueueProducer> logger)
        {
            _rabbitMqConnection = rabbitMqConnection;            
            _logger = logger;
        }

        public void Publish(object message, string queueName)
        {
            IConnection connection = _rabbitMqConnection.Connect();

            using(var channel = connection.CreateModel())            
            {
                channel.QueueDeclare(
                    queue: queueName, 
                    durable: true,
                    exclusive: false, 
                    autoDelete: false, 
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: null,
                    body: body);

                _logger.LogDebug($"QueueProducer.Publish: {message}");
            }
        }
    }
}