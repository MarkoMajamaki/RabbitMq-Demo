using System.Net.Http;
using System.Threading.Tasks;
using Messaging;
using Microsoft.AspNetCore.Mvc;
using Service1.Services;

namespace Service1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ITestService2 _testService2;
        private readonly ITestService3 _testService3;
        private readonly IPubSubService _pubSubService;

        public TestController(ITestService2 testService2, ITestService3 testService3, IPubSubService pubSubService)
        {
            _testService2 = testService2;
            _testService3 = testService3;
            _pubSubService = pubSubService;
        }

        [HttpGet]
        public string Get()
        {
            return "Service 1 is running!";
        }

        [HttpGet("service2")]
        public async Task<string> CallService2()
        {
            return await _testService2.CallAsync();
        }

        [HttpGet("service3")]
        public async Task<string> CallService3()
        {
            return await _testService3.CallAsync();
        }

        [HttpPost("Publish/{message}")]
        public void Publish(string message)
        {
            _pubSubService.Publish(message, "testQueue");
        }
    }
}