using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.Common.Constants;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.Common.Models.Responses;
using WoWMarketWatcher.UI.Constants;
using static WoWMarketWatcher.Common.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.UI.Services
{
    public class WatchListService : IWatchListService
    {
        private readonly IHttpClientFactory httpClientFactory;

        private readonly ILogger<WatchListService> logger;

        private readonly JsonSerializerOptions defaultSerializerSettings = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        public WatchListService(IHttpClientFactory httpClientFactory, ILogger<WatchListService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task<List<WatchListDto>> GetWatchListsForUserAsync(int userId, string correlationId = null)
        {
            var sourceName = GetSourceName();
            correlationId ??= Guid.NewGuid().ToString();

            var httpClient = this.httpClientFactory.CreateClient(HttpClientNames.AuthorizedWoWMarketWatcherAPI);
            httpClient.DefaultRequestHeaders.Add(AppHeaderNames.CorrelationId, correlationId);

            var watchLists = await httpClient.GetFromJsonAsync<CursorPaginatedResponse<WatchListDto>>($"users/{userId}/watchLists?includeEdges=false", this.defaultSerializerSettings);

            return watchLists.Nodes.ToList();
        }
    }
}