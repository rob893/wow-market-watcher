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
    public sealed class RealmRepository : Repository<Realm, RealmQueryParameters>, IRealmRepository
    {
        private readonly IMemoryCache cache;

        public RealmRepository(DataContext context, IMemoryCache cache) : base(context)
        {
            this.cache = cache;
        }

        public override async Task<Realm?> FirstOrDefaultAsync(Expression<Func<Realm, bool>> condition, bool track = true)
        {
            if (!track)
            {
                var realmLookup = await this.GetRealmsFromCacheAsync();

                return realmLookup.Values.FirstOrDefault(condition.Compile());
            }

            var realm = await base.FirstOrDefaultAsync(condition, track);

            return realm;
        }

        public override async Task<Realm?> GetByIdAsync(int id, bool track = true)
        {
            if (!track)
            {
                var realmLookup = await this.GetRealmsFromCacheAsync();

                return realmLookup.ContainsKey(id) ? realmLookup[id] : default;
            }

            var realm = await base.GetByIdAsync(id, track);

            return realm;
        }

        public override async Task<List<Realm>> SearchAsync(Expression<Func<Realm, bool>> condition, bool track = true)
        {
            if (!track)
            {
                var realmLookup = await this.GetRealmsFromCacheAsync();

                return realmLookup.Values.Where(condition.Compile()).ToList();
            }

            var realms = await base.SearchAsync(condition, track);

            return realms;
        }

        public override async Task<CursorPaginatedList<Realm, int>> SearchAsync(RealmQueryParameters searchParams, bool track = true)
        {
            if (!track)
            {
                var realmLookup = await this.GetRealmsFromCacheAsync();

                return Filter(realmLookup.Values, searchParams).ToCursorPaginatedList(searchParams);
            }

            var realms = await base.SearchAsync(searchParams, track);

            return realms;
        }

        protected override IQueryable<Realm> AddWhereClauses(IQueryable<Realm> query, RealmQueryParameters searchParams)
        {
            // Keep both this and add where clauses for performance.
            // MySQL is case insensitive by default but adding ignore case adds lots of unneeded overhead to database query.
            // However, case insentitivity is needed for cached results.
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            if (searchParams.Name != null)
            {
                query = query.Where(realm => realm.Name == searchParams.Name);
            }

            if (searchParams.NameLike != null)
            {
                query = query.Where(realm => realm.Name.Contains(searchParams.NameLike));
            }

            if (searchParams.Timezone != null)
            {
                query = query.Where(realm => realm.Timezone == searchParams.Timezone);
            }

            if (searchParams.Region != null)
            {
                query = query.Where(realm => realm.Region == searchParams.Region);
            }

            return query;
        }

        private static IEnumerable<Realm> Filter(IEnumerable<Realm> query, RealmQueryParameters searchParams)
        {
            // Keep both this and add where clauses for performance.
            // MySQL is case insensitive by default but adding ignore case adds lots of unneeded overhead to database query.
            // However, case insentitivity is needed for cached results.
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            if (searchParams.Name != null)
            {
                query = query.Where(realm => realm.Name.Equals(searchParams.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (searchParams.NameLike != null)
            {
                query = query.Where(realm => realm.Name.Contains(searchParams.NameLike, StringComparison.OrdinalIgnoreCase));
            }

            if (searchParams.Timezone != null)
            {
                query = query.Where(realm => realm.Timezone.Equals(searchParams.Timezone, StringComparison.OrdinalIgnoreCase));
            }

            if (searchParams.Region != null)
            {
                query = query.Where(realm => realm.Region.Equals(searchParams.Region, StringComparison.OrdinalIgnoreCase));
            }

            return query;
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