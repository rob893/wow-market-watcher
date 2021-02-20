using System.Linq;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.Common.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public class WatchListRepository : Repository<WatchList, CursorPaginationParameters>, IWatchListRepository
    {
        public WatchListRepository(DataContext context) : base(context) { }

        public Task<CursorPagedList<WatchList, int>> GetWatchListsForUserAsync(int userId, CursorPaginationParameters searchParams)
        {
            var query = this.Context.WatchLists
                .Where(list => list.UserId == userId);

            return CursorPagedList<WatchList, int>.CreateAsync(query, searchParams);
        }
    }
}