namespace WoWMarketWatcher.API.Models.Settings
{
    public record SwaggerAuthSettings
    {
        public string Username { get; init; } = default!;
        public string Password { get; init; } = default!;
        public bool RequireAuth { get; init; }
    }
}