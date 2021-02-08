namespace WoWMarketWatcher.API.Models.Settings
{
    public record MySQLSettings
    {
        public string DefaultConnection { get; init; } = default!;
        public bool EnableSensitiveDataLogging { get; init; }
        public bool EnableDetailedErrors { get; init; }
    }
}