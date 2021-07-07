using Common;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Publisher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IPublisher _publisher;
        private readonly ILogger _logger;

        public MessageController(
            IPublisher publisher, 
            ILogger<MessageController> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

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
    }
}