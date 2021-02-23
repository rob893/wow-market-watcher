using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Models.Settings;
using WoWMarketWatcher.Common.Extensions;

namespace WoWMarketWatcher.API.Services
{
    public class BlizzardService : IBlizzardService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly BlizzardSettings settings;
        private readonly IMemoryCache cache;
        private readonly ILogger<BlizzardService> logger;

        public BlizzardService(IHttpClientFactory httpClientFactory, IOptions<BlizzardSettings> settings, IMemoryCache cache, ILogger<BlizzardService> logger)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetAccessTokenAsync(string correlationId, bool forceRefresh = false)
        {
            var sourceName = this.GetSourceName();

            if (!forceRefresh && this.cache.TryGetValue<string>(CacheKeys.BlizzardAPIAccessTokenKey, out var cachedToken))
            {
                this.logger.LogInformation(sourceName, correlationId, "Some stuff");
                return cachedToken;
            }

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri(this.settings.OAuthUrl),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>> { new KeyValuePair<string?, string?>("grant_type", "client_credentials") })
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.settings.ClientId}:{this.settings.ClientSecret}")));

            using var response = await httpClient.SendAsync(request);

            var contentString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unable to get Blizzard token: Status: {response.StatusCode} Reason: {contentString}");
            }

            var content = JsonConvert.DeserializeObject<BlizzardTokenResponse>(contentString);

            if (content == null)
            {
                throw new HttpRequestException("Unable to deserialize JSON");
            }

            this.cache.Set(CacheKeys.BlizzardAPIAccessTokenKey, content.AccessToken, TimeSpan.FromSeconds(content.ExpiresIn - 10));

            return content.AccessToken;
        }

        public async Task<BlizzardAuctionsResponse> GetAuctionsAsync(int realmId, string correlationId)
        {
            var accessToken = await this.GetAccessTokenAsync(correlationId);

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            using var request = new HttpRequestMessage(HttpMethod.Get, $"data/wow/connected-realm/{realmId}/auctions?namespace=dynamic-us&locale=en_US");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);

            using var response = await httpClient.SendAsync(request);

            var contentString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unable to get Blizzard auctions: Status: {response.StatusCode} Reason: {contentString}");
            }

            var content = JsonConvert.DeserializeObject<BlizzardAuctionsResponse>(contentString);

            return content ?? throw new HttpRequestException("Unable to deserialize JSON");
        }

        public async Task<BlizzardWoWItem> GetWoWItemAsync(int itemId, string correlationId)
        {
            var accessToken = await this.GetAccessTokenAsync(correlationId);

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            using var request = new HttpRequestMessage(HttpMethod.Get, $"data/wow/item/{itemId}?namespace=static-us&locale=en_US");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);

            using var response = await httpClient.SendAsync(request);

            var contentString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unable to get Blizzard item: Status: {response.StatusCode} Reason: {contentString}");
            }

            if (string.IsNullOrWhiteSpace(contentString))
            {
                throw new HttpRequestException("Blizzard API returned null or an empty string as body.");
            }

            var content = JsonConvert.DeserializeObject<BlizzardWoWItem>(contentString);

            return content ?? throw new HttpRequestException("Unable to deserialize JSON");
        }

        public async Task<BlizzardSearchResponse<BlizzardLocaleWoWItem>> GetWoWItemsAsync(IEnumerable<int> itemIds, string correlationId)
        {
            if (itemIds.Count() > 100)
            {
                throw new ArgumentException("itemIds max count is 100");
            }

            var accessToken = await this.GetAccessTokenAsync(correlationId);

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            var itemIdsQuery = itemIds.Aggregate("", (prev, curr) => $"{(string.IsNullOrWhiteSpace(prev) ? "id=" : $"{prev}||")}{curr}");

            using var request = new HttpRequestMessage(HttpMethod.Get, $"data/wow/search/item?namespace=static-us&locale=en_US&{itemIdsQuery}");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);

            using var response = await httpClient.SendAsync(request);

            var contentString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unable to get Blizzard item: Status: {response.StatusCode} Reason: {contentString}");
            }

            if (string.IsNullOrWhiteSpace(contentString))
            {
                throw new HttpRequestException("Blizzard API returned null or an empty string as body.");
            }

            var content = JsonConvert.DeserializeObject<BlizzardSearchResponse<BlizzardLocaleWoWItem>>(contentString);

            return content ?? throw new HttpRequestException("Unable to deserialize JSON");
        }

        public async Task<BlizzardSearchResponse<BlizzardConnectedRealm>> GetConnectedRealmsAsync(string correlationId)
        {
            var sourceName = this.GetSourceName();

            var accessToken = await this.GetAccessTokenAsync(correlationId);

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            using var request = new HttpRequestMessage(HttpMethod.Get, $"data/wow/search/connected-realm?namespace=dynamic-us&locale=en_US");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);

            using var response = await httpClient.SendAsync(request);

            var contentString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unable to get Blizzard realms: Status: {response.StatusCode} Reason: {contentString}");
            }

            if (string.IsNullOrWhiteSpace(contentString))
            {
                throw new HttpRequestException("Blizzard API returned null or an empty string as body.");
            }

            var content = JsonConvert.DeserializeObject<BlizzardSearchResponse<BlizzardConnectedRealm>>(contentString);

            return content ?? throw new HttpRequestException("Unable to deserialize JSON");
        }
    }
}