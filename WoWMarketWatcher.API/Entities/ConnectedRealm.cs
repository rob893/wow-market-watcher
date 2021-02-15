using System.Collections.Generic;
using WoWMarketWatcher.Common.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class ConnectedRealm : IIdentifiable<int>
    {
        public int Id { get; init; }
        public List<Realm> Realms { get; init; } = new List<Realm>();
    }
}