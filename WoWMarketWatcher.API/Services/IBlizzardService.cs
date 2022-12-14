using System.Collections.Generic;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Models.Responses.Blizzard;

namespace WoWMarketWatcher.API.Services
{
    public interface IBlizzardService
    {
        Task<string> GetAccessTokenAsync(bool forceRefresh = false);

        Task<BlizzardAuctionsResponse> GetAuctionsAsync(int realmId);

        Task<BlizzardAuctionsResponse> GetCommodityAuctionsAsync();

        Task<BlizzardWoWItem> GetWoWItemAsync(int itemId);

        Task<BlizzardSearchResponse<BlizzardLocaleWoWItem>> GetWoWItemsAsync(IEnumerable<int> itemIds);

        Task<BlizzardSearchResponse<BlizzardConnectedRealm>> GetConnectedRealmsAsync(int pageNumber = 1, int pageSize = 100);

        Task<IEnumerable<BlizzardConnectedRealm>> GetAllConnectedRealmsAsync();

        Task<BlizzardWoWTokenResponse> GetWoWTokenPriceAsync();
    }
}