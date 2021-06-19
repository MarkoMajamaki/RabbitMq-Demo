using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Service1.Services
{
    public interface ITestService2
    {
        Task<string> CallAsync();
    }

    public class TestService2 : ITestService2
    {
        private readonly HttpClient _client;

        public TestService2(HttpClient client)
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