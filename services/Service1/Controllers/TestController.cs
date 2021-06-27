using Microsoft.AspNetCore.Mvc;

namespace Service1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Service 1 is running!";
        }
    }
}