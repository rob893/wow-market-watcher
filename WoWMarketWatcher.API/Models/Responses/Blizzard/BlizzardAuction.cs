using Newtonsoft.Json;

namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardAuction
    {
        public long Id { get; init; }
        public BlizzardAuctionItem Item { get; init; } = default!;
        public long Quantity { get; init; }
        [JsonProperty("unit_price")]
        public long? UnitPrice { get; init; }
        public long? Buyout { get; init; }
        public long? Bid { get; init; }
        [JsonProperty("time_left")]
        public string TimeLeft { get; init; } = default!;
    }
}