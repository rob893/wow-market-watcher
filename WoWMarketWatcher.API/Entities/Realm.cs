using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class Realm : IIdentifiable<int>
    {
        public int Id { get; init; }
        public string Name { get; set; } = default!;
        public int ConnectedRealmId { get; set; }
        public ConnectedRealm ConnectedRealm { get; set; } = default!;
        public bool IsTournament { get; set; }
        public string Locale { get; set; } = default!;
        public string Timezone { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string Region { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Type { get; set; } = default!;
    }
}