using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Models.Settings;

using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Services
{
    public sealed class BlizzardService : IBlizzardService
    {
        private readonly IHttpClientFactory httpClientFactory;

        private readonly BlizzardSettings settings;

        private readonly IMemoryCache cache;

        private readonly ICorrelationIdService correlationIdService;

        private readonly ILogger<BlizzardService> logger;

        public BlizzardService(
            IHttpClientFactory httpClientFactory,
            IOptions<BlizzardSettings> settings,
            IMemoryCache cache,
            ICorrelationIdService correlationIdService,
            ILogger<BlizzardService> logger)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string CorrelationId => this.correlationIdService.CorrelationId;

        public async Task<string> GetAccessTokenAsync(bool forceRefresh = false)
        {
            var sourceName = GetSourceName();

            this.logger.LogDebug(sourceName, this.CorrelationId, "Method started.");

            if (!forceRefresh && this.cache.TryGetValue<string>(CacheKeys.BlizzardAPIAccessTokenKey, out var cachedToken))
            {
                this.logger.LogInformation(sourceName, this.CorrelationId, "Returning cached Blizzard access token.");
                return cachedToken;
            }

            this.logger.LogDebug(sourceName, this.CorrelationId, "Fetching new Blizzard access token.");

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            using var request = new HttpRequestMessage
            {
                RequestUri = this.settings.OAuthUrl,
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>> { new KeyValuePair<string?, string?>("grant_type", "client_credentials") })
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.settings.ClientId}:{this.settings.ClientSecret}")));
            request.Headers.Add(AppHeaderNames.CorrelationId, this.CorrelationId);

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

            this.logger.LogInformation(sourceName, this.CorrelationId, "New Blizzard access token fetched and cached. Returning new token.");
            this.logger.LogInformation(sourceName, this.CorrelationId, "Method complete.");

            return content.AccessToken;
        }

        public Task<BlizzardAuctionsResponse> GetAuctionsAsync(int realmId)
        {
            return this.SendRequestAsync<BlizzardAuctionsResponse>(
                HttpMethod.Get,
                $"data/wow/connected-realm/{realmId}/auctions?namespace=dynamic-us&locale=en_US");
        }

        public Task<BlizzardAuctionsResponse> GetCommodityAuctionsAsync()
        {
            return this.SendRequestAsync<BlizzardAuctionsResponse>(
                HttpMethod.Get,
                $"data/wow/auctions/commodities?namespace=dynamic-us&locale=en_US");
        }

        public Task<BlizzardWoWItem> GetWoWItemAsync(int itemId)
        {
            return this.SendRequestAsync<BlizzardWoWItem>(
                HttpMethod.Get,
                $"data/wow/item/{itemId}?namespace=static-us&locale=en_US");
        }

        public Task<BlizzardSearchResponse<BlizzardLocaleWoWItem>> GetWoWItemsAsync(IEnumerable<int> itemIds)
        {
            if (itemIds.Count() > 100)
            {
                throw new ArgumentException("itemIds max count is 100");
            }

            var itemIdsQuery = itemIds.Aggregate("", (prev, curr) => $"{(string.IsNullOrWhiteSpace(prev) ? "id=" : $"{prev}||")}{curr}");

            return this.SendRequestAsync<BlizzardSearchResponse<BlizzardLocaleWoWItem>>(
                HttpMethod.Get,
                $"data/wow/search/item?namespace=static-us&locale=en_US&{itemIdsQuery}");
        }

        public Task<BlizzardSearchResponse<BlizzardConnectedRealm>> GetConnectedRealmsAsync(int pageNumber = 1, int pageSize = 100)
        {
            return this.SendRequestAsync<BlizzardSearchResponse<BlizzardConnectedRealm>>(
                HttpMethod.Get,
                $"data/wow/search/connected-realm?namespace=dynamic-us&locale=en_US&_page={pageNumber}&_pageSize={pageSize}");
        }

        public async Task<IEnumerable<BlizzardConnectedRealm>> GetAllConnectedRealmsAsync()
        {
            var firstPage = await this.GetConnectedRealmsAsync(1, 100);

            if (firstPage.PageCount <= 1)
            {
                return firstPage.Results.Select(r => r.Data);
            }

            var tasks = new List<Task<BlizzardSearchResponse<BlizzardConnectedRealm>>>(firstPage.PageCount - 1);

            for (var i = firstPage.Page + 1; i <= firstPage.PageCount; i++)
            {
                tasks.Add(this.GetConnectedRealmsAsync(i, 100));
            }

            var total = (await Task.WhenAll(tasks)).Aggregate(firstPage.Results.Select(r => r.Data), (prev, curr) => prev.Concat(curr.Results.Select(r => r.Data)));

            return total;
        }

        public Task<BlizzardWoWTokenResponse> GetWoWTokenPriceAsync()
        {
            return this.SendRequestAsync<BlizzardWoWTokenResponse>(
                HttpMethod.Get,
                "data/wow/token/index?locale=en_US&namespace=dynamic-us");
        }

        private async Task<T> SendRequestAsync<T>(HttpMethod method, string url, bool isRetry = false)
        {
            var sourceName = GetSourceName();

            var accessToken = await this.GetAccessTokenAsync(isRetry);

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            using var request = new HttpRequestMessage(method, url);

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
            request.Headers.Add(AppHeaderNames.CorrelationId, this.CorrelationId);

            using var response = await httpClient.SendAsync(request);

            var contentString = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized && !isRetry)
            {
                this.logger.LogWarning(sourceName, this.CorrelationId, $"Request to {url} failed due to being unauthorizied. Refreshing token and retrying.");
                return await this.SendRequestAsync<T>(method, url, true);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Blizzard API request failed: Status: {response.StatusCode} Reason: {contentString}");
            }

            if (string.IsNullOrWhiteSpace(contentString))
            {
                throw new HttpRequestException("Blizzard API returned null or an empty string as body.");
            }

            var content = JsonConvert.DeserializeObject<T>(contentString);

            return content ?? throw new HttpRequestException("Unable to deserialize JSON");
        }
    }
}