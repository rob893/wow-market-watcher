namespace WoWMarketWatcher.Common.Models.QueryParameters
{
    public record CursorPaginationParameters
    {
        public int? First { get; init; }
        public string? After { get; init; }
        public int? Last { get; init; }
        public string? Before { get; init; }
        public bool IncludeTotal { get; init; } = false;
        public bool IncludeNodes { get; init; } = true;
        public bool IncludeEdges { get; init; } = true;
    }
}