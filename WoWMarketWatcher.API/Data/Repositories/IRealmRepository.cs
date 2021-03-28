using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IRealmRepository : IRepository<Realm, RealmQueryParameters> { }
}