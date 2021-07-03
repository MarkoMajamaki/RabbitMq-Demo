using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Common
{
    public interface IConnectionProvider
    {
        IConnection Connect();
        void Close();
    }

    public class ConnectionProvider : IConnectionProvider
    {
        private RabbitMqSettings _rabbitMqOptions;
        private readonly ILogger _logger;
        private IConnection _connection;

        public ConnectionProvider(
            IOptions<RabbitMqSettings> rabbitMqOptions,
            ILogger<ConnectionProvider> logger)
        {
            _rabbitMqOptions = rabbitMqOptions.Value;            
            _logger = logger;
        }

        /// <summary>
        /// Connect to RabbitMq
        /// </summary>
        public IConnection Connect()
        {   
            if (_connection != null)
            {
                return _connection;
            }         

            try
            {
                string connectionUrl = "amqp://" + _rabbitMqOptions.UserName + ":" + _rabbitMqOptions.Password + "@" + _rabbitMqOptions.HostName + ":" + _rabbitMqOptions.Port + "/";

                _logger.LogInformation($"RabbitMq try create connection to: {connectionUrl}");

                ConnectionFactory factory = new ConnectionFactory()
                {
                    Uri = new Uri(connectionUrl),
                };

                _connection = factory.CreateConnection();

                _logger.LogInformation($"RabbitMq connection create SUCCESSFUL to: {connectionUrl}");

                return _connection;
            }
            catch (Exception e)
            {
                _logger.LogError($"RabbitMq connection create FAILED! {e.Message}");
 
                return null;
            }
        }

        /// <summary>
        /// Close connection
        /// </summary>
        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}