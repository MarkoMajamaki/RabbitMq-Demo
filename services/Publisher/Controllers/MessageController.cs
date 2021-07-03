using Common;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace Publisher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IPublisher _publisher;

        public MessageController(IPublisher publisher)
        {
            _publisher = publisher;
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
    }
}