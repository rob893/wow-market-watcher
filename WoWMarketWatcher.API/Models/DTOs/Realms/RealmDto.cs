namespace WoWMarketWatcher.API.Models.DTOs.Realms
{
    public record RealmDto : IIdentifiable<int>
    {
        public int Id { get; init; }

        public string Name { get; init; } = default!;

        public int ConnectedRealmId { get; init; }

        public bool IsTournament { get; init; }

        public string Locale { get; init; } = default!;

        public string Timezone { get; init; } = default!;

        public string Slug { get; init; } = default!;

        public string Region { get; init; } = default!;

        public string Category { get; init; } = default!;

        public string Type { get; init; } = default!;
    }
}