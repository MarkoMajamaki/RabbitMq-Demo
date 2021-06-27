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
        private readonly IQueueConsumer _queueConsumer;
        private readonly IDirectExchangeConsumer _directExchangeConsumer;
        private readonly ITopicExchangeConsumer _topicExchangeConsumer;
        private readonly IFanoutExchangeConsumer _fanoutExchangeConsumer;

        public ReceiveMessagesService(
            ILogger<ReceiveMessagesService> logger,
            IQueueConsumer queueConsumer, 
            IDirectExchangeConsumer directExchangeConsumer,
            ITopicExchangeConsumer topicExchangeConsumer,
            IFanoutExchangeConsumer fanoutExchangeConsumer)
        {
            _logger = logger;
            _queueConsumer = queueConsumer;
            _directExchangeConsumer = directExchangeConsumer;
            _topicExchangeConsumer = topicExchangeConsumer;
            _fanoutExchangeConsumer = fanoutExchangeConsumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _queueConsumer.Subscribe("testQueue", (message) =>
            {
                _logger.LogDebug(message.ToString());
            });

            _directExchangeConsumer.Subscribe("direct-exchange", "direct-queue", "routingKey.test", (message) =>
            {
                _logger.LogDebug(message.ToString());
            });

            _topicExchangeConsumer.Subscribe("topic-exchange", "topic-queue-1", "*.test", (message) =>
            {
                _logger.LogDebug(message.ToString());
            });
            _topicExchangeConsumer.Subscribe("topic-exchange", "topic-queue-2", "routingKey.*", (message) =>
            {
                _logger.LogDebug(message.ToString());
            });

            _fanoutExchangeConsumer.Subscribe("fanout-exchange", "fanout-queue", "", (message) =>
            {
                _logger.LogDebug(message.ToString());
            });

            return Task.CompletedTask;
        }
    }
}