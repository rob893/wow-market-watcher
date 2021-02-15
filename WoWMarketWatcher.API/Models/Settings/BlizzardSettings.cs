namespace WoWMarketWatcher.API.Models.Settings
{
    public record BlizzardSettings
    {
        public string BaseUrl { get; init; } = default!;
        public string OAuthUrl { get; init; } = default!;
        public string ClientId { get; init; } = default!;
        public string ClientSecret { get; init; } = default!;
    }
}