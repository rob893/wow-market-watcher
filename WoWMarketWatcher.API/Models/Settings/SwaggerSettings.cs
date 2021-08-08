namespace WoWMarketWatcher.API.Models.Settings
{
    public record SwaggerSettings
    {
        public SwaggerAuthSettings AuthSettings { get; init; } = default!;

        public bool Enabled { get; init; }
    }
}