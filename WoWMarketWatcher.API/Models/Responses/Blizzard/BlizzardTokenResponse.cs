using Newtonsoft.Json;

namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; init; } = default!;

        [JsonProperty("token_type")]
        public string TokenType { get; init; } = default!;

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; init; }
    }
}