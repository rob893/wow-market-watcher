namespace WoWMarketWatcher.API.Models.Settings
{
    public record PullAuctionDataBackgroundJobSettings : BackgroundJobSettingsEntry
    {
        public bool AlwaysProcessCertainItemsEnabled { get; init; }
    }
}