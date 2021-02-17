using System;

namespace WoWMarketWatcher.Common.Models.DTOs
{
    public record AuctionTimeSeriesEntryDto : IIdentifiable<long>
    {
        public long Id { get; init; }
        public int WoWItemId { get; init; }
        public int ConnectedRealmId { get; init; }
        public DateTime Timestamp { get; init; }
        public long TotalAvailableForAuction { get; init; }
        public long AveragePrice { get; init; }
        public long MinPrice { get; init; }
        public long MaxPrice { get; init; }
        public long Price25Percentile { get; init; }
        public long Price50Percentile { get; init; }
        public long Price75Percentile { get; init; }
        public long Price95Percentile { get; init; }
        public long Price99Percentile { get; init; }
    }
}