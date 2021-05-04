namespace WoWMarketWatcher.API.Models.Settings
{
    public record SendGridSettings
    {
        public string APIKey { get; init; } = default!;
    }
}