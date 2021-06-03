using System;

namespace WoWMarketWatcher.API.Models.Settings
{
    public record BlizzardSettings
    {
        public Uri BaseUrl { get; init; } = default!;
        public Uri OAuthUrl { get; init; } = default!;
        public string ClientId { get; init; } = default!;
        public string ClientSecret { get; init; } = default!;
    }
}