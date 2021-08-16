using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public sealed class AlertRepository : Repository<Alert, CursorPaginationQueryParameters>, IAlertRepository
    {
        public AlertRepository(DataContext context) : base(context) { }

        public Task<CursorPaginatedList<Alert, int>> GetAlertsForUserAsync(int userId, CursorPaginationQueryParameters searchParams, bool track = true)
        {
            var query = this.Context.Alerts
                .Where(alert => alert.UserId == userId);

            if (!track)
            {
                query = query.AsNoTracking();
            }

            query = this.AddIncludes(query);

            return query.ToCursorPaginatedListAsync(searchParams);
        }

        protected override IQueryable<Alert> AddIncludes(IQueryable<Alert> query)
        {
            return query
                .Include(alert => alert.Conditions)
                .Include(alert => alert.Actions);
        }
    }
}