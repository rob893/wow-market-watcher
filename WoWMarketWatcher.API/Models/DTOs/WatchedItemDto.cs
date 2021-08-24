using Newtonsoft.Json;

namespace WoWMarketWatcher.API.Models.DTOs
{
    public record WatchedItemDto : IIdentifiable<int>
    {
        public int Id { get; init; }

        public int WatchListId { get; init; }

        public int ConnectedRealmId { get; init; }

        [JsonProperty("wowItemId")]
        public int WoWItemId { get; init; }

        [JsonProperty("wowItem")]
        public WoWItemDto WoWItem { get; init; } = default!;
    }
}