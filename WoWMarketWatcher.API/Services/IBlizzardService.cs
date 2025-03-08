using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Models.Responses.Blizzard;

namespace WoWMarketWatcher.API.Services
{
    public interface IBlizzardService
    {
        Task<string> GetAccessTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);

        Task<BlizzardAuctionsResponse> GetAuctionsAsync(int realmId, CancellationToken cancellationToken = default);

        Task<BlizzardAuctionsResponse> GetCommodityAuctionsAsync(CancellationToken cancellationToken = default);

        Task<BlizzardWoWItem> GetWoWItemAsync(int itemId, CancellationToken cancellationToken = default);

        Task<BlizzardSearchResponse<BlizzardLocaleWoWItem>> GetWoWItemsAsync(IEnumerable<int> itemIds, CancellationToken cancellationToken = default);

        Task<BlizzardSearchResponse<BlizzardConnectedRealm>> GetConnectedRealmsAsync(int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default);

        Task<IEnumerable<BlizzardConnectedRealm>> GetAllConnectedRealmsAsync(CancellationToken cancellationToken = default);

        Task<BlizzardWoWTokenResponse> GetWoWTokenPriceAsync(CancellationToken cancellationToken = default);
    }
}