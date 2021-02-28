using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.UI.Constants;

namespace WoWMarketWatcher.UI.Services
{
    public class TestService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<TestService> logger;
        private readonly JsonSerializerOptions defaultSerializerSettings = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        public TestService(IHttpClientFactory httpClientFactory, ILogger<TestService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task Test()
        {
            var httpClient = this.httpClientFactory.CreateClient(HttpClientNames.AuthorizedWoWMarketWatcherAPI);
            var response = await httpClient.GetFromJsonAsync<object>("test?status=500&statusAfter=200&per=3", this.defaultSerializerSettings);

            this.logger.LogInformation(response.ToString());
        }
    }
}