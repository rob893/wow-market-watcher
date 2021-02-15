namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardWoWItem
    {
        public int Id { get; init; }
        public string Name { get; init; } = default!;
    }
}