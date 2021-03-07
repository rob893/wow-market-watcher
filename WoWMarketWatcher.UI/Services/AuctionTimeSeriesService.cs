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
using WoWMarketWatcher.Common.Extensions;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.Common.Models.QueryParameters;
using WoWMarketWatcher.Common.Models.Responses;
using WoWMarketWatcher.UI.Constants;
using static WoWMarketWatcher.Common.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.UI.Services
{
    public class AuctionTimeSeriesService
    {
        private readonly IHttpClientFactory httpClientFactory;

        private readonly ILogger<AuctionTimeSeriesService> logger;

        private readonly JsonSerializerOptions defaultSerializerSettings = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        public AuctionTimeSeriesService(IHttpClientFactory httpClientFactory, ILogger<AuctionTimeSeriesService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task<List<AuctionTimeSeriesEntryDto>> GetAuctionTimeSeriesAsync(AuctionTimeSeriesQueryParameters queryParameters, string correlationId = null)
        {
            var sourceName = GetSourceName();
            correlationId ??= Guid.NewGuid().ToString();

            var query = queryParameters.ToQueryString();

            var httpClient = this.httpClientFactory.CreateClient(HttpClientNames.AuthorizedWoWMarketWatcherAPI);
            httpClient.DefaultRequestHeaders.Add(AppHeaderNames.CorrelationId, correlationId);

            var watchLists = await httpClient.GetFromJsonAsync<CursorPaginatedResponse<AuctionTimeSeriesEntryDto, long>>($"wow/auctionTimeSeries?{query}", this.defaultSerializerSettings);

            return watchLists.Nodes.ToList();
        }
    }
}