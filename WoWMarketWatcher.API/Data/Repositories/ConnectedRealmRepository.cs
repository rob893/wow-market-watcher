using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public sealed class ConnectedRealmRepository : Repository<ConnectedRealm, CursorPaginationQueryParameters>, IConnectedRealmRepository
    {
        private readonly IMemoryCache cache;

        public ConnectedRealmRepository(DataContext context, IMemoryCache cache) : base(context)
        {
            this.cache = cache;
        }

        public override async Task<List<ConnectedRealm>> SearchAsync(Expression<Func<ConnectedRealm, bool>> condition, bool track = true)
        {
            if (!track)
            {
                var (connectedRealmLookup, _) = await this.GetFromCacheAsync();

                return connectedRealmLookup.Values.Where(condition.Compile()).ToList();
            }

            var res = await base.SearchAsync(condition, track);

            return res;
        }

        public async Task<CursorPaginatedList<Realm, int>> GetRealmsForConnectedRealmAsync(int connectedRealmId, RealmQueryParameters searchParams, bool track = true)
        {
            if (!track)
            {
                var (_, realmLookup) = await this.GetFromCacheAsync();

                return realmLookup.Values.ToCursorPaginatedList(searchParams);
            }

            var res = await this.Context.Realms
                .Where(realm => realm.ConnectedRealmId == connectedRealmId)
                .ToCursorPaginatedListAsync(searchParams);

            return res;
        }

        protected override IQueryable<ConnectedRealm> AddIncludes(IQueryable<ConnectedRealm> query)
        {
            query = query.Include(connectedRealm => connectedRealm.Realms);

            return query;
        }

        private async Task<(Dictionary<int, ConnectedRealm> connectedRealmLookup, Dictionary<int, Realm> realmLookup)> GetFromCacheAsync()
        {
            if (!this.cache.TryGetValue(CacheKeys.ConnectedRealmsLookup, out Dictionary<int, ConnectedRealm> connectedRealmLookup) ||
                !this.cache.TryGetValue(CacheKeys.RealmLookup, out Dictionary<int, Realm> realmLookup))
            {
                var connectedRealms = await this.Context.ConnectedRealms.AsNoTracking().Include(realm => realm.Realms).ToListAsync();
                connectedRealmLookup = connectedRealms.ToDictionary(connectedRealm => connectedRealm.Id);
                realmLookup = connectedRealms
                    .SelectMany(connectedRealm => connectedRealm.Realms)
                    .DistinctBy(realm => realm.Id)
                    .ToDictionary(realm => realm.Id);

                this.cache.Set(CacheKeys.ConnectedRealmsLookup, connectedRealmLookup, TimeSpan.FromHours(1));
                this.cache.Set(CacheKeys.RealmLookup, realmLookup, TimeSpan.FromHours(1));
            }

            return (connectedRealmLookup, realmLookup);
        }
    }
}