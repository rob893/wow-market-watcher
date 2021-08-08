using System.Threading.Tasks;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IAlertRepository : IRepository<Alert, CursorPaginationQueryParameters>
    {
        Task<CursorPaginatedList<Alert, int>> GetAlertsForUserAsync(int userId, CursorPaginationQueryParameters searchParams);
    }
}