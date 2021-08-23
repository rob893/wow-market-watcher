namespace WoWMarketWatcher.API.Models.Entities
{
    public class WatchedItem : IIdentifiable<int>
    {
        public int Id { get; set; }

        public int WatchListId { get; set; }

        public WatchList WatchList { get; set; } = default!;

        public int ConnectedRealmId { get; set; }

        public ConnectedRealm ConnectedRealm { get; set; } = default!;

        public int WoWItemId { get; set; }

        public WoWItem WoWItem { get; set; } = default!;
    }
}