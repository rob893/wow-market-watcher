using System.Collections.Generic;
using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class WoWItem : IIdentifiable<int>
    {
        public int Id { get; init; }
        public string Name { get; init; } = default!;
        public bool IsEquippable { get; init; }
        public bool IsStackable { get; init; }
        public int Level { get; init; }
        public int RequiredLevel { get; init; }
        public long SellPrice { get; init; }
        public int PurchaseQuantity { get; init; }
        public int PurchasePrice { get; init; }
        public string ItemClass { get; init; } = default!;
        public string ItemSubclass { get; init; } = default!;
        public string Quality { get; init; } = default!;
        public string InventoryType { get; init; } = default!;
        public int MaxCount { get; init; }
        public List<WatchList> WatchedIn { get; init; } = new List<WatchList>();
    }
}