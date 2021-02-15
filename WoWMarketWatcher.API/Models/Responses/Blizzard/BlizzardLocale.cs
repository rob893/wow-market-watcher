using Newtonsoft.Json;

namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardLocale
    {
        [JsonProperty("en_US")]
        public string EnUS { get; init; } = default!;
    }
}