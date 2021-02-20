using System.Collections.Generic;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Models.Responses.Blizzard;

namespace WoWMarketWatcher.API.Services
{
    public interface IBlizzardService
    {
        Task<string> GetAccessTokenAsync();
        Task<BlizzardAuctionsResponse> GetAuctionsAsync(int realmId);
        Task<BlizzardWoWItem> GetWoWItemAsync(int itemId);
        Task<BlizzardSearchResponse<BlizzardLocaleWoWItem>> GetWoWItemsAsync(IEnumerable<int> itemIds);
        Task<BlizzardSearchResponse<BlizzardConnectedRealm>> GetConnectedRealmsAsync();
    }
}