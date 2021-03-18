using System.Collections.Generic;
using WoWMarketWatcher.Common.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class WatchList : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = default!;
        public int ConnectedRealmId { get; set; }
        public ConnectedRealm ConnectedRealm { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public List<WoWItem> WatchedItems { get; set; } = new List<WoWItem>();
    }
}