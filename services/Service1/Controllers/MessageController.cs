using Messaging;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace Service1.Controllers
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
            _publisher.Queue(message: message, queue: "testQueue");
        }

        [HttpPost("DirectExchangePublish/{message}")]
        public void DirectExchangePublish(string message)
        {
            _publisher.Direct(message: message, exchange: "direct-exchange", routingKey: "routingKey.test");
        }

        [HttpPost("TopicExchangePublish/{message}")]
        public void TopicExchangePublish(string message)
        {
            _publisher.Topic(message: message, exchange: "topic-exchange", routingKey: "routingKey.test");
        }

        [HttpPost("FanoutExchangePublish/{message}")]
        public void FanoutExchangePublish(string message)
        {
            // _publisher.Fanout(message: message, exchange: "fanout-exchange", routingKey: "", headers: {{""}});
        }
    }
}