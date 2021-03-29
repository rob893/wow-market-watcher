namespace WoWMarketWatcher.API.Models.Settings
{
    public record BackgroundJobSettings
    {
        public BackgroundJobSettingsEntry PullAuctionDataBackgroundJob { get; init; } = default!;
        public BackgroundJobSettingsEntry RemoveOldDataBackgroundJob { get; init; } = default!;
        public BackgroundJobSettingsEntry PullRealmDataBackgroundJob { get; init; } = default!;
    }
}