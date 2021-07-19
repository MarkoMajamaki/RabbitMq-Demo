using Common;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Publisher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IPublisher _publisher;
        private readonly IPubSubService _pubSubService;
        private readonly IRpcService _rpcService;
        private readonly ILogger _logger;

        public MessageController(
            IPublisher publisher,
            ILogger<MessageController> logger,
            IPubSubService pubSubService, 
            IRpcService rpcService)
        {
            _publisher = publisher;
            _logger = logger;
            _pubSubService = pubSubService;
            _rpcService = rpcService;
        }

        // IPublisher

        [HttpPost("QueuePublish/{message}")]
        public void QueuePublish(string message)
        {
            _publisher.Queue(message, "simple-queue");
        }

        [HttpPost("DirectExchangePublish/{message}")]
        public void DirectExchangePublish(string message)
        {
            _publisher.Direct(message, "direct-exchange", "routingKey.test");
        }

        [HttpPost("TopicExchangePublish/{message}")]
        public void TopicExchangePublish(string message)
        {
            _publisher.Topic(message, "topic-exchange", "routingKey.test");
        }

        [HttpPost("FanoutExchangePublish/{message}")]
        public void FanoutExchangePublish(string message)
        {
            var headers = new Dictionary<string, object>();
            headers.Add("account", "update");

            _publisher.Fanout(message, "fanout-exchange", "account.new", headers);
        }

        [HttpPost("HeaderExchangePublish/{message}")]
        public void HeaderExchangePublish(string message)
        {
            var headers = new Dictionary<string, object>();
            headers.Add("account", "new");

            _publisher.Header(message, "header-exchange", headers);
        }

        [HttpPost("Rpc/{message}")]
        public async void Rpc(string message)
        {
            var response = await _publisher.Rpc(message, "rpc-queue");

            _logger.LogDebug($"Rpc response: {response}");
        }

        // PubSubService

        [HttpPost("PubSubServicePublish/{message}")]
        public void PubSubServicePublish(string message)
        {
            _pubSubService.Publish(new TestRequest(message), new() {{"message", "send"}});
        }

        // RpcService

        [HttpPost("RpcServicePublish/{message}")]
        public async void RpcServicePublish(string message)
        {
            TestResponse response = await _rpcService.Invoke<TestResponse>(new TestRequest(message), "rpcservice");

            _logger.LogDebug($"RpcService response: {response.Response}");
        }
    }
}