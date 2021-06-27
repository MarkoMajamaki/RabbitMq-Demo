using Messaging;
using Microsoft.AspNetCore.Mvc;

namespace Service1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IQueueProducer _messageProducer;
        private readonly IDirectExchangeProducer _directExchangeProducer;
        private readonly IFanoutExchangeProducer _fanoutExchangeProducer;
        private readonly ITopicExchangeProducer _topicExchangeProducer;

        public MessageController( 
            IQueueProducer messageProducer,
            IDirectExchangeProducer directExchangeProducer,
            IFanoutExchangeProducer fanoutExchangeProducer,
            ITopicExchangeProducer topicExchangeProducer)
        {
            _messageProducer = messageProducer;
            _directExchangeProducer = directExchangeProducer;
            _fanoutExchangeProducer = fanoutExchangeProducer;
            _topicExchangeProducer = topicExchangeProducer;
        }

        [HttpPost("QueuePublish/{message}")]
        public void QueuePublish(string message)
        {
            _messageProducer.Publish(message, "testQueue");
        }

        [HttpPost("DirectExchangePublish/{message}")]
        public void DirectExchangePublish(string message)
        {
            _directExchangeProducer.Publish(message, "direct-exchange", "routingKey.test");
        }

        [HttpPost("TopicExchangePublish/{message}")]
        public void TopicExchangePublish(string message)
        {
            _topicExchangeProducer.Publish(message, "topic-exchange", "routingKey.test");
        }

        [HttpPost("FanoutExchangePublish/{message}")]
        public void FanoutExchangePublish(string message)
        {
            _fanoutExchangeProducer.Publish(message, "fanout-exchange", "");
        }
    }
}