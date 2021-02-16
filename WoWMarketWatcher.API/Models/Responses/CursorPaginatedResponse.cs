using System;
using System.Collections.Generic;
using System.Linq;
using WoWMarketWatcher.API.Core;
using Newtonsoft.Json;
using WoWMarketWatcher.Common.Models;
using WoWMarketWatcher.Common.Models.QueryParameters;

namespace WoWMarketWatcher.API.Models.Responses
{
    public class CursorPaginatedResponse<TEntity, TEntityKey>
        where TEntity : class, IIdentifiable<TEntityKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Edge<TEntity>>? Edges { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<TEntity>? Nodes { get; set; }
        public PageInfo PageInfo { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalCount { get; set; }

        private readonly Func<TEntityKey, string> ConvertIdToBase64;


        public CursorPaginatedResponse(IEnumerable<TEntity> items, string? startCursor, string? endCursor, bool hasNextPage,
            bool hasPreviousPage, int? totalCount, Func<TEntityKey, string> ConvertIdToBase64, bool includeNodes = true, bool includeEdges = true)
        {
            if (!includeEdges && !includeNodes)
            {
                throw new ArgumentException("Both includeEdges and includeNodes cannot be false.");
            }

            this.ConvertIdToBase64 = ConvertIdToBase64;

            if (includeEdges)
            {
                SetEdges(items);
            }

            if (includeNodes)
            {
                Nodes = items.ToList();
            }

            PageInfo = new PageInfo
            {
                StartCursor = startCursor,
                EndCursor = endCursor,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };
            TotalCount = totalCount;
        }

        public CursorPaginatedResponse(CursorPagedList<TEntity, TEntityKey> items, Func<TEntityKey, string> ConvertIdToBase64, bool includeNodes = true, bool includeEdges = true) :
            this(items, items.StartCursor, items.EndCursor, items.HasNextPage, items.HasPreviousPage, items.TotalCount, ConvertIdToBase64, includeNodes, includeEdges)
        { }

        public static CursorPaginatedResponse<TDestination, int> CreateFrom<TSource, TDestination>(CursorPagedList<TSource, int> items, Func<IEnumerable<TSource>, IEnumerable<TDestination>> mappingFunction, bool includeNodes = true, bool includeEdges = true)
            where TSource : class, IIdentifiable<int>
            where TDestination : class, IIdentifiable<int>
        {
            var mappedItems = mappingFunction(items);

            return new CursorPaginatedResponse<TDestination, int>(mappedItems, items.StartCursor, items.EndCursor, items.HasNextPage, items.HasPreviousPage, items.TotalCount, Id => Convert.ToBase64String(BitConverter.GetBytes(Id)), includeNodes, includeEdges);
        }

        public static CursorPaginatedResponse<TDestination, int> CreateFrom<TSource, TDestination>(CursorPagedList<TSource, int> items, Func<IEnumerable<TSource>, IEnumerable<TDestination>> mappingFunction, CursorPaginationParameters searchParams)
            where TSource : class, IIdentifiable<int>
            where TDestination : class, IIdentifiable<int>
        {
            return CursorPaginatedResponse<TDestination, int>.CreateFrom(items, mappingFunction, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TSource, int> CreateFrom<TSource>(CursorPagedList<TSource, int> items, CursorPaginationParameters searchParams)
            where TSource : class, IIdentifiable<int>
        {
            return CursorPaginatedResponse<TSource, int>.CreateFrom(items, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TSource, int> CreateFrom<TSource>(CursorPagedList<TSource, int> items, bool includeNodes = true, bool includeEdges = true)
            where TSource : class, IIdentifiable<int>
        {
            return new CursorPaginatedResponse<TSource, int>(items, items.StartCursor, items.EndCursor, items.HasNextPage, items.HasPreviousPage, items.TotalCount, Id => Convert.ToBase64String(BitConverter.GetBytes(Id)), includeNodes, includeEdges);
        }

        private void SetEdges(IEnumerable<TEntity> items)
        {
            Edges = items.Select(item => new Edge<TEntity>
            {
                Cursor = ConvertIdToBase64(item.Id),
                Node = item
            });
        }
    }

    public class CursorPaginatedResponse<TEntity> : CursorPaginatedResponse<TEntity, int>
        where TEntity : class, IIdentifiable<int>
    {
        public CursorPaginatedResponse(IEnumerable<TEntity> items, string? startCursor, string? endCursor,
            bool hasNextPage, bool hasPreviousPage, int? totalCount, bool includeNodes = true, bool includeEdges = true) : base(
            items,
            startCursor,
            endCursor,
            hasNextPage,
            hasPreviousPage,
            totalCount,
            Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
            includeNodes,
            includeEdges
        )
        { }
    }

    public class Edge<T>
    {
        public string Cursor { get; set; } = default!;
        public T Node { get; set; } = default!;
    }

    public class PageInfo
    {
        public string? StartCursor { get; set; }
        public string? EndCursor { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}