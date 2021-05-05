namespace WoWMarketWatcher.API.Models.Settings
{
    public record SendGridSettings
    {
        public string APIKey { get; init; } = default!;
        public string Sender { get; init; } = default!;
        public string SenderName { get; init; } = default!;
    }
}