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
        private readonly IPubSubService _pubSubService;

        public ReceiveMessagesService(
            ILogger<ReceiveMessagesService> logger,
            IPubSubService pubSubService)
        {
            _logger = logger;
            _pubSubService = pubSubService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReceiveMessagesService.ExecuteAsync");

            _pubSubService.Subscribe("testQueue", (message) =>
            {
                _logger.LogInformation(message.ToString());
            });

            return Task.FromResult(true);
        }
    }
}