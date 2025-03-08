using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.Settings
{
    public sealed record SwaggerSettings
    {
        public required SwaggerAuthSettings AuthSettings { get; init; }

        public required bool Enabled { get; init; }

        public required List<string> SupportedApiVersions { get; init; } = [];
    }
}