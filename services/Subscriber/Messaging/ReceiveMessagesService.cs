using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Subscriber
{
    public class ReceiveMessagesService : BackgroundService
    {
        private readonly ILogger<ReceiveMessagesService> _logger;
        private readonly Common.ISubscriber _subscriber;
        private readonly IPubSubService _pubSubService;
        private readonly IRpcService _rpcService;


        public ReceiveMessagesService(
            ILogger<ReceiveMessagesService> logger,
            Common.ISubscriber subscriber,
            IPubSubService pubSubService, 
            IRpcService rpcService)
        {
            _logger = logger;
            _subscriber = subscriber;
            _pubSubService = pubSubService;
            _rpcService = rpcService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // ISubscriber

            _subscriber.Queue((message) =>
            {
                _logger.LogInformation(message.ToString());
                return true;
            }, "simple-queue"); 

            _subscriber.Direct((message) =>
            {
                _logger.LogInformation(message.ToString());
                return true;
            }, "direct-queue", "direct-exchange", "routingKey.test");

            _subscriber.Topic((message) =>
            {
                _logger.LogInformation(message.ToString());
                return true;
            }, "topic-queue-1", "topic-exchange", "*.test");

            _subscriber.Topic((message) =>
            {
                _logger.LogInformation(message.ToString());
                return true;
            }, "topic-queue-2", "topic-exchange", "routingKey.*");

            _subscriber.Fanout((message) =>
            {
                _logger.LogInformation(message.ToString());
                return true;
            }, "fanout-queue", "fanout-exchange");

            var header = new Dictionary<string, object> { { "account", "update" } };

            _subscriber.Headers((message) =>
            {
                _logger.LogInformation(message.ToString());
                return true;
            }, "header-queue", "header-exchange", header, true);

            _subscriber.Rpc((message) =>
            {
                _logger.LogInformation(message.ToString());
                return "RPC message handled!";
            }, "rpc-queue");

            // PubSubService
            _pubSubService.Subscribe<TestRequest>(message =>
            {
                _logger.LogInformation(message.Message);
                return Task.FromResult(true);
            }, new() {{"message", "send"}});

            // RpcService
            _rpcService.Handle<TestRequest, TestResponse>(message => {
                return new TestResponse("RPC call response");
            }, "rpcservice");

            return Task.CompletedTask;
        }
    }
}