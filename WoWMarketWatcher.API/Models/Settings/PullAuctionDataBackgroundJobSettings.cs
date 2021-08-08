using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.Settings
{
    public record PullAuctionDataBackgroundJobSettings : BackgroundJobSettingsEntry
    {
        public bool AlwaysProcessCertainItemsEnabled { get; init; }

        public Dictionary<string, string[]> AlwayProcessItemClasses { get; init; } = new();
    }
}