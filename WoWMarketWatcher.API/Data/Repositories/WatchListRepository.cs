using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public sealed class WatchListRepository : Repository<WatchList, CursorPaginationQueryParameters>, IWatchListRepository
    {
        public WatchListRepository(DataContext context) : base(context) { }

        public Task<CursorPaginatedList<WatchList, int>> GetWatchListsForUserAsync(int userId, CursorPaginationQueryParameters searchParams)
        {
            var query = this.Context.WatchLists
                .Where(list => list.UserId == userId);

            query = this.AddIncludes(query);

            return query.ToCursorPaginatedListAsync(searchParams);
        }

        protected override IQueryable<WatchList> AddIncludes(IQueryable<WatchList> query)
        {
            return query.Include(watchList => watchList.WatchedItems);
        }
    }
}