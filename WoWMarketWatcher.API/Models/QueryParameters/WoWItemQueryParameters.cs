namespace WoWMarketWatcher.API.Models.QueryParameters
{
    public record WoWItemQueryParameters : CursorPaginationParameters
    {
        public string? Quality { get; init; }
        public string? ItemClass { get; init; }
        public string? ItemSubclass { get; init; }
        public string? InventoryType { get; init; }
        public string? NameLike { get; init; }
        public string? Name { get; init; }
    }
}