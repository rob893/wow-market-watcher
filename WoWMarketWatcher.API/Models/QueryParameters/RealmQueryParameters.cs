namespace WoWMarketWatcher.API.Models.QueryParameters
{
    public record RealmQueryParameters : CursorPaginationQueryParameters
    {
        public string? Name { get; init; }
        public string? NameLike { get; init; }
        public string? Timezone { get; init; }
        public string? Region { get; init; }
    }
}