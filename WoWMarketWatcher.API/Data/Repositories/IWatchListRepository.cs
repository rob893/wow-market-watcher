using System.Threading.Tasks;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IWatchListRepository : IRepository<WatchList, CursorPaginationQueryParameters>
    {
        Task<CursorPaginatedList<WatchList, int>> GetWatchListsForUserAsync(int userId, CursorPaginationQueryParameters searchParams, bool track = true);
    }
}