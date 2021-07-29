using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class Realm : IIdentifiable<int>
    {
        public int Id { get; init; }

        [MaxLength(50)]
        public string Name { get; set; } = default!;

        public int ConnectedRealmId { get; set; }

        public ConnectedRealm ConnectedRealm { get; set; } = default!;

        public bool IsTournament { get; set; }

        [MaxLength(50)]
        public string Locale { get; set; } = default!;

        [MaxLength(50)]
        public string Timezone { get; set; } = default!;

        [MaxLength(50)]
        public string Slug { get; set; } = default!;

        [MaxLength(50)]
        public string Region { get; set; } = default!;

        [MaxLength(50)]
        public string Category { get; set; } = default!;

        [MaxLength(50)]
        public string Type { get; set; } = default!;
    }
}