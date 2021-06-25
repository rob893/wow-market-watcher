using System.Collections.Generic;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Models.Responses.Blizzard;

namespace WoWMarketWatcher.API.Services
{
    public interface IBlizzardService
    {
        Task<string> GetAccessTokenAsync(string correlationId, bool forceRefresh = false);
        Task<BlizzardAuctionsResponse> GetAuctionsAsync(int realmId, string correlationId);
        Task<BlizzardWoWItem> GetWoWItemAsync(int itemId, string correlationId);
        Task<BlizzardSearchResponse<BlizzardLocaleWoWItem>> GetWoWItemsAsync(IEnumerable<int> itemIds, string correlationId);
        Task<BlizzardSearchResponse<BlizzardConnectedRealm>> GetConnectedRealmsAsync(string correlationId, int pageNumber = 1, int pageSize = 100);
        Task<IEnumerable<BlizzardConnectedRealm>> GetAllConnectedRealmsAsync(string correlationId);
        Task<BlizzardWoWTokenResponse> GetWoWTokenPriceAsync(string correlationId);
    }
}