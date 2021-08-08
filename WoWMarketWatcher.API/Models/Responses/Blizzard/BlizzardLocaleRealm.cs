using Newtonsoft.Json;

namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardLocaleRealm
    {
        public int Id { get; init; }

        public BlizzardLocale Name { get; init; } = default!;

        [JsonProperty("is_tournament")]
        public bool IsTournament { get; init; }

        public string Locale { get; init; } = default!;

        public string Timezone { get; init; } = default!;

        public string Slug { get; init; } = default!;

        public BlizzardLocaleName Region { get; init; } = default!;

        public BlizzardLocale Category { get; init; } = default!;

        public BlizzardLocaleName Type { get; init; } = default!;
    }
}