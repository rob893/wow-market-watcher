using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public class ConnectedRealmRepository : Repository<ConnectedRealm, CursorPaginationParameters>, IConnectedRealmRepository
    {
        public ConnectedRealmRepository(DataContext context) : base(context) { }

        public Task<CursorPagedList<Realm, int>> GetRealmsForConnectedRealmAsync(int connectedRealmId, RealmQueryParameters searchParams)
        {
            var query = this.Context.Realms
                .Where(realm => realm.ConnectedRealmId == connectedRealmId);

            return CursorPagedList<Realm, int>.CreateAsync(query, searchParams);
        }

        protected override IQueryable<ConnectedRealm> AddIncludes(IQueryable<ConnectedRealm> query)
        {
            query = query.Include(connectedRealm => connectedRealm.Realms);

            return query;
        }
    }
}