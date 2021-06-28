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
            }, "testQueue"); 

            _subscriber.Direct((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "direct-exchange", "direct-queue", "routingKey.test");

            _subscriber.Topic((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "topic-exchange", "topic-queue-1", "*.test");

            _subscriber.Topic((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "topic-exchange", "topic-queue-2", "routingKey.*");

            _subscriber.Fanout((message) =>
            {
                _logger.LogDebug(message);
                return true;
            }, "fanout-exchange", "fanout-queue");

            return Task.CompletedTask;
        }
    }
}