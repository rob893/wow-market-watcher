using System.Linq;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.Common.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public class RealmRepository : Repository<Realm, RealmQueryParameters>
    {
        public RealmRepository(DataContext context) : base(context) { }

        protected override IQueryable<Realm> AddWhereClauses(IQueryable<Realm> query, RealmQueryParameters searchParams)
        {
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
    }
}