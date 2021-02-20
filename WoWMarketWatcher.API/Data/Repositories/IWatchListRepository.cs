using System.Threading.Tasks;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.Common.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IWatchListRepository : IRepository<WatchList, CursorPaginationParameters>
    {
        Task<CursorPagedList<WatchList, int>> GetWatchListsForUserAsync(int userId, CursorPaginationParameters searchParams);
    }
}