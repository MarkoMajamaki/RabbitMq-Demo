using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging
{
    public interface IPubSubService
    {
        void Publish(object message, string queueName);
        void Subscribe(string queueName, Action<object> received);
    }

    public class PubSubService : IPubSubService
    {
        private RabbitMqSettings _rabbitMqOptions;
        private IConnection _connection;
        private readonly ILogger _logger;

        public PubSubService(
            IOptions<RabbitMqSettings> rabbitMqOptions,
            ILogger<PubSubService> logger)
        {
            _rabbitMqOptions = rabbitMqOptions.Value;            
            _logger = logger;
        }

        /// <summary>
        /// Publish message
        /// </summary>
        public void Publish(object message, string queueName)
        {
            if (_connection == null)
            {
                _connection = CreateConnection();
            }

            using(var channel = _connection.CreateModel())            
            {
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish(exchange: "",
                                    routingKey: queueName,
                                    basicProperties: null,
                                    body: body);

                Console.WriteLine(" [x] Sent {0}", message);
            }
        }

        /// <summary>
        /// Subscribe message
        /// </summary>
        public void Subscribe(string queueName, Action<object> received)
        {
            if (_connection == null)
            {
                _connection = CreateConnection();
            }

            using(var channel = _connection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    received(message);
                };

                channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Create connection
        /// </summary>
        private IConnection CreateConnection()
        {            
            IConnection connection;

            try
            {
                string connectionUrl = "amqp://" + _rabbitMqOptions.UserName + ":" + _rabbitMqOptions.Password + "@" + _rabbitMqOptions.HostName + ":" + _rabbitMqOptions.Port + "/";

                _logger.LogInformation($"RabbitMq try create connection to: {connectionUrl}");

                ConnectionFactory factory = new ConnectionFactory()
                {
                    Uri = new Uri(connectionUrl),
                };

                connection = factory.CreateConnection();

                _logger.LogInformation($"RabbitMq connection create SUCCESSFUL to: {connectionUrl}");

                return connection;
            }
            catch (Exception e)
            {
                _logger.LogError($"RabbitMq connection create FAILED! {e.Message}");
 
                return null;
            }
        }
    }
}