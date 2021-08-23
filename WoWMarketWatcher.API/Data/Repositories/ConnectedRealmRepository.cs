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

        public override async Task<ConnectedRealm?> FirstOrDefaultAsync(Expression<Func<ConnectedRealm, bool>> condition, bool track = true)
        {
            if (!track)
            {
                var connectedRealmLookup = await this.GetConnectedRealmsFromCacheAsync();

                return connectedRealmLookup.Values.FirstOrDefault(condition.Compile());
            }

            var realm = await base.FirstOrDefaultAsync(condition, track);

            return realm;
        }

        public override async Task<ConnectedRealm?> GetByIdAsync(int id, bool track = true)
        {
            if (!track)
            {
                var connectedRealmLookup = await this.GetConnectedRealmsFromCacheAsync();

                return connectedRealmLookup.ContainsKey(id) ? connectedRealmLookup[id] : default;
            }

            var realm = await base.GetByIdAsync(id, track);

            return realm;
        }

        public override async Task<List<ConnectedRealm>> SearchAsync(Expression<Func<ConnectedRealm, bool>> condition, bool track = true)
        {
            if (!track)
            {
                var connectedRealmLookup = await this.GetConnectedRealmsFromCacheAsync();

                return connectedRealmLookup.Values.Where(condition.Compile()).ToList();
            }

            var realms = await base.SearchAsync(condition, track);

            return realms;
        }

        public override async Task<CursorPaginatedList<ConnectedRealm, int>> SearchAsync(CursorPaginationQueryParameters searchParams, bool track = true)
        {
            if (!track)
            {
                var connectedRealmLookup = await this.GetConnectedRealmsFromCacheAsync();

                return connectedRealmLookup.Values.ToCursorPaginatedList(searchParams);
            }

            var realms = await base.SearchAsync(searchParams, track);

            return realms;
        }

        public async Task<CursorPaginatedList<Realm, int>> GetRealmsForConnectedRealmAsync(int connectedRealmId, RealmQueryParameters searchParams, bool track = true)
        {
            if (!track)
            {
                var realmLookup = await this.GetRealmsFromCacheAsync();

                return realmLookup.Values.ToCursorPaginatedList(searchParams);
            }

            var realms = await this.Context.Realms
                .Where(realm => realm.ConnectedRealmId == connectedRealmId)
                .ToCursorPaginatedListAsync(searchParams);

            return realms;
        }

        protected override IQueryable<ConnectedRealm> AddIncludes(IQueryable<ConnectedRealm> query)
        {
            query = query.Include(connectedRealm => connectedRealm.Realms);

            return query;
        }

        private async Task<Dictionary<int, ConnectedRealm>> GetConnectedRealmsFromCacheAsync()
        {
            if (!this.cache.TryGetValue(CacheKeys.ConnectedRealmsLookup, out Dictionary<int, ConnectedRealm> connectedRealmLookup))
            {
                var connectedRealms = await this.Context.ConnectedRealms.AsNoTracking().Include(realm => realm.Realms).ToListAsync();
                connectedRealmLookup = connectedRealms.ToDictionary(connectedRealm => connectedRealm.Id);
                var realmLookup = connectedRealms
                    .SelectMany(connectedRealm => connectedRealm.Realms)
                    .DistinctBy(realm => realm.Id)
                    .ToDictionary(realm => realm.Id);

                this.cache.Set(CacheKeys.ConnectedRealmsLookup, connectedRealmLookup, TimeSpan.FromHours(1));
                this.cache.Set(CacheKeys.RealmLookup, realmLookup, TimeSpan.FromHours(1));
            }

            return connectedRealmLookup;
        }

        private async Task<Dictionary<int, Realm>> GetRealmsFromCacheAsync()
        {
            if (!this.cache.TryGetValue(CacheKeys.RealmLookup, out Dictionary<int, Realm> realmLookup))
            {
                var connectedRealms = await this.Context.ConnectedRealms.AsNoTracking().Include(realm => realm.Realms).ToListAsync();
                var connectedRealmLookup = connectedRealms.ToDictionary(connectedRealm => connectedRealm.Id);
                realmLookup = connectedRealms
                    .SelectMany(connectedRealm => connectedRealm.Realms)
                    .DistinctBy(realm => realm.Id)
                    .ToDictionary(realm => realm.Id);

                this.cache.Set(CacheKeys.ConnectedRealmsLookup, connectedRealmLookup, TimeSpan.FromHours(1));
                this.cache.Set(CacheKeys.RealmLookup, realmLookup, TimeSpan.FromHours(1));
            }

            return realmLookup;
        }
    }
}