namespace WoWMarketWatcher.API.Models.Settings
{
    public record BackgroundJobSettingsEntry
    {
        public bool Enabled { get; init; }
        public string Schedule { get; init; } = default!;
    }
}