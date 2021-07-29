using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class WatchList : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = default!;

        public int ConnectedRealmId { get; set; }

        public ConnectedRealm ConnectedRealm { get; set; } = default!;

        [MaxLength(255)]
        public string Name { get; set; } = default!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        public List<WoWItem> WatchedItems { get; set; } = new List<WoWItem>();
    }
}