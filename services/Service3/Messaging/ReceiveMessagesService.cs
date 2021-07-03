using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Service3
{
    public class ReceiveMessagesService : BackgroundService
    {
        private readonly ILogger<ReceiveMessagesService> _logger;
        private readonly ISubscriber _subscriber;

        public ReceiveMessagesService(
            ILogger<ReceiveMessagesService> logger,
            ISubscriber subscriber)
        {
            _logger = logger;
            _subscriber = subscriber;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _subscriber.Queue((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "simple-queue"); 

            _subscriber.Direct((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "direct-queue", "direct-exchange", "routingKey.test");

            _subscriber.Topic((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "topic-queue-1", "topic-exchange", "*.test");

            _subscriber.Topic((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "topic-queue-2", "topic-exchange", "routingKey.*");

            _subscriber.Fanout((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "fanout-queue", "fanout-exchange");

            var header = new Dictionary<string, object> { { "account", "update" } };

            _subscriber.Headers((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "header-queue", "header-exchange", header, true);

            return Task.CompletedTask;
        }
    }
}