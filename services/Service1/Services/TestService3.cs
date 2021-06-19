using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Service1.Services
{
    public interface ITestService3
    {
        Task<string> CallAsync();
    }

    public class TestService3 : ITestService3
    {
        private readonly HttpClient _client;
        
        public TestService3(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> CallAsync()
        {
            HttpResponseMessage result = await _client.GetAsync("test");
            return await result.Content.ReadAsStringAsync();
        }
    }
}