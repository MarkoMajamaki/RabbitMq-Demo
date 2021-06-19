using Microsoft.AspNetCore.Mvc;

namespace Service3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Service 3 is running!";
        }
    }
}