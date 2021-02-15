namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardAuctionItem
    {
        public int Id { get; init; }
        public int? Context { get; init; }
    }
}