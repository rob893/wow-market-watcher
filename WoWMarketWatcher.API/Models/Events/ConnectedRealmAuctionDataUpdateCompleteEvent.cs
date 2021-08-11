namespace WoWMarketWatcher.API.Models.Events
{
    public record ConnectedRealmAuctionDataUpdateCompleteEvent
    {
        public int ConnectedRealmId { get; init; }
    }
}