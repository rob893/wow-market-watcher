using System.Collections.Generic;
using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class ConnectedRealm : IIdentifiable<int>
    {
        public int Id { get; init; }
        public string Population { get; set; } = default!;
        public List<AuctionTimeSeriesEntry> AuctionTimeSeries { get; init; } = new List<AuctionTimeSeriesEntry>();
        public List<Realm> Realms { get; init; } = new List<Realm>();
    }
}