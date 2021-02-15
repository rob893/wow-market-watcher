namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardSearchResult<TResult>
    {
        public TResult Data { get; init; } = default!;
    }
}