using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WoWMarketWatcher.API.Models.Responses
{
    public record CursorPaginatedResponse<TEntity, TEntityKey>
        where TEntity : class, IIdentifiable<TEntityKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Edge<TEntity>>? Edges { get; init; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<TEntity>? Nodes { get; init; }
        public PageInfo PageInfo { get; init; } = default!;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalCount { get; init; }
    }

    public record CursorPaginatedResponse<TEntity> : CursorPaginatedResponse<TEntity, int>
        where TEntity : class, IIdentifiable<int>
    { }

    public record Edge<T>
    {
        public string Cursor { get; init; } = default!;
        public T Node { get; init; } = default!;
    }

    public record PageInfo
    {
        public string? StartCursor { get; init; }
        public string? EndCursor { get; init; }
        public bool HasNextPage { get; init; }
        public bool HasPreviousPage { get; init; }
    }
}