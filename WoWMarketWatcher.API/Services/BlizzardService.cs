using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Models.Settings;

namespace WoWMarketWatcher.API.Services
{
    public class BlizzardService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly BlizzardSettings settings;

        private BlizzardTokenResponse? cachedTokenResponse;

        public BlizzardService(IHttpClientFactory httpClientFactory, IOptions<BlizzardSettings> settings)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<BlizzardTokenResponse> GetTokenAsync()
        {
            if (this.cachedTokenResponse != null && !this.cachedTokenResponse.IsExpired)
            {
                return cachedTokenResponse;
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

            this.cachedTokenResponse = content;

            return content;
        }

        public async Task<BlizzardAuctionsResponse> GetAuctionsAsync(int realmId)
        {
            var accessToken = await this.GetTokenAsync();

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            using var request = new HttpRequestMessage(HttpMethod.Get, $"data/wow/connected-realm/{realmId}/auctions?namespace=dynamic-us&locale=en_US");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken.AccessToken);

            using var response = await httpClient.SendAsync(request);

            var contentString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unable to get Blizzard auctions: Status: {response.StatusCode} Reason: {contentString}");
            }

            var content = JsonConvert.DeserializeObject<BlizzardAuctionsResponse>(contentString);

            return content ?? throw new HttpRequestException("Unable to deserialize JSON");
        }

        public async Task<BlizzardWoWItem> GetWoWItemAsync(int itemId)
        {
            var accessToken = await this.GetTokenAsync();

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            using var request = new HttpRequestMessage(HttpMethod.Get, $"data/wow/item/{itemId}?namespace=static-us&locale=en_US");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken.AccessToken);

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

        public async Task<BlizzardSearchResponse<BlizzardLocaleWoWItem>> GetWoWItemsAsync(IEnumerable<int> itemIds)
        {
            if (itemIds.Count() > 100)
            {
                throw new ArgumentException("itemIds max count is 100");
            }

            var accessToken = await this.GetTokenAsync();

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            var itemIdsQuery = itemIds.Aggregate("", (prev, curr) => $"{(string.IsNullOrWhiteSpace(prev) ? "id=" : $"{prev}||")}{curr}");

            using var request = new HttpRequestMessage(HttpMethod.Get, $"data/wow/search/item?namespace=static-us&locale=en_US&{itemIdsQuery}");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken.AccessToken);

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

        public async Task<BlizzardSearchResponse<BlizzardConnectedRealm>> GetConnectedRealmsAsync()
        {
            var accessToken = await this.GetTokenAsync();

            var httpClient = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            using var request = new HttpRequestMessage(HttpMethod.Get, $"data/wow/search/connected-realm?namespace=dynamic-us&locale=en_US");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken.AccessToken);

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