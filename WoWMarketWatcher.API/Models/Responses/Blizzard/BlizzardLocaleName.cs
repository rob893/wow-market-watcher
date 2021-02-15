namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardLocaleName
    {
        public BlizzardLocale Name { get; init; } = default!;
    }
}