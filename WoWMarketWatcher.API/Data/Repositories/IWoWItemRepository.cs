using System.Collections.Generic;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IWoWItemRepository : IRepository<WoWItem, WoWItemQueryParameters>
    {
        Task<List<string>> GetItemQualitiesAsync();
        Task<List<string>> GetItemClassesAsync();
        Task<List<string>> GetItemSubclassesAsync();
        Task<List<string>> GetItemInventoryTypesAsync();
    }
}