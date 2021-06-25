using Newtonsoft.Json;

namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardWoWTokenResponse
    {
        [JsonProperty("last_updated_timestamp")]
        public long LastUpdatedTimestamp { get; init; }
        public long Price { get; init; }
    }
}