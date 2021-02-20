using System.Threading.Tasks;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.Common.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IConnectedRealmRepository : IRepository<ConnectedRealm, CursorPaginationParameters>
    {
        Task<CursorPagedList<Realm, int>> GetRealmsForConnectedRealmAsync(int connectedRealmId, RealmQueryParameters searchParams);
    }
}