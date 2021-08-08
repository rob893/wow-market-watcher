using System;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class AuctionTimeSeriesEntry : IIdentifiable<long>
    {
        public long Id { get; init; }

        public int WoWItemId { get; init; }

        public int ConnectedRealmId { get; init; }

        public DateTime Timestamp { get; init; }

        public long TotalAvailableForAuction { get; set; }

        public long AveragePrice { get; set; }

        public long MinPrice { get; set; }

        public long MaxPrice { get; set; }

        public long Price25Percentile { get; set; }

        public long Price50Percentile { get; set; }

        public long Price75Percentile { get; set; }

        public long Price95Percentile { get; set; }

        public long Price99Percentile { get; set; }
    }
}