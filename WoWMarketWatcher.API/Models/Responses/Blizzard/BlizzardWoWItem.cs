using Newtonsoft.Json;

namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardWoWItem
    {
        public int Id { get; init; }
        public string Name { get; init; } = default!;
        [JsonProperty("is_equippable")]
        public bool IsEquippable { get; init; }
        [JsonProperty("is_stackable")]
        public bool IsStackable { get; init; }
        public int Level { get; init; }
        [JsonProperty("required_level")]
        public int RequiredLevel { get; init; }
        [JsonProperty("sell_price")]
        public long SellPrice { get; init; }
        [JsonProperty("purchase_quantity")]
        public int PurchaseQuantity { get; init; }
        [JsonProperty("purchase_price")]
        public int PurchasePrice { get; init; }
        [JsonProperty("item_class")]
        public BlizzardLocalizedName ItemClass { get; init; } = default!;
        [JsonProperty("item_subclass")]
        public BlizzardLocalizedName ItemSubclass { get; init; } = default!;
        public BlizzardLocalizedName Quality { get; init; } = default!;
        [JsonProperty("inventory_type")]
        public BlizzardLocalizedName InventoryType { get; init; } = default!;
        [JsonProperty("max_count")]
        public int MaxCount { get; init; }
    }
}