using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardSearchResponse<TResult>
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int MaxPageSize { get; init; }
        public int PageCount { get; init; }
        public List<BlizzardSearchResult<TResult>> Results { get; init; } = new List<BlizzardSearchResult<TResult>>();
    }
}