using System;
using System.Collections.Generic;
using System.Linq;
using WoWMarketWatcher.Common.Models;
using WoWMarketWatcher.Common.Models.QueryParameters;
using WoWMarketWatcher.Common.Models.Responses;

namespace WoWMarketWatcher.API.Core
{
    public static class CursorPaginatedResponseFactory
    {
        public static CursorPaginatedResponse<TEntity, TEntityKey> CreateFrom<TEntity, TEntityKey>(
            IEnumerable<TEntity> items,
            string? startCursor,
            string? endCursor,
            bool hasNextPage,
            bool hasPreviousPage,
            int? totalCount,
            Func<TEntityKey, string> ConvertIdToBase64,
            bool includeNodes = true,
            bool includeEdges = true)
                where TEntity : class, IIdentifiable<TEntityKey>
                where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        {
            if (!includeEdges && !includeNodes)
            {
                throw new ArgumentException("Both includeEdges and includeNodes cannot be false.");
            }

            return new CursorPaginatedResponse<TEntity, TEntityKey>
            {
                Edges = includeEdges ? GetEdges(items, ConvertIdToBase64) : null,
                Nodes = includeNodes ? items.ToList() : null,
                PageInfo = new PageInfo
                {
                    StartCursor = startCursor,
                    EndCursor = endCursor,
                    HasNextPage = hasNextPage,
                    HasPreviousPage = hasPreviousPage
                },
                TotalCount = totalCount
            };
        }

        public static CursorPaginatedResponse<TEntity, TEntityKey> CreateFrom<TEntity, TEntityKey>(
            CursorPagedList<TEntity, TEntityKey> items,
            Func<TEntityKey, string> ConvertIdToBase64,
            bool includeNodes = true,
            bool includeEdges = true)
                where TEntity : class, IIdentifiable<TEntityKey>
                where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        {
            return CreateFrom(
                items,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.TotalCount,
                ConvertIdToBase64,
                includeNodes,
                includeEdges);
        }

        public static CursorPaginatedResponse<TDestination, int> CreateFrom<TSource, TDestination>(
            CursorPagedList<TSource, int> items, Func<IEnumerable<TSource>,
            IEnumerable<TDestination>> mappingFunction,
            bool includeNodes = true,
            bool includeEdges = true)
                where TSource : class, IIdentifiable<int>
                where TDestination : class, IIdentifiable<int>
        {
            var mappedItems = mappingFunction(items);

            return CreateFrom<TDestination, int>(
                mappedItems,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.TotalCount,
                Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
                includeNodes,
                includeEdges);
        }

        public static CursorPaginatedResponse<TDestination, int> CreateFrom<TSource, TDestination>(
            CursorPagedList<TSource, int> items,
            Func<IEnumerable<TSource>, IEnumerable<TDestination>> mappingFunction,
            CursorPaginationParameters searchParams)
                where TSource : class, IIdentifiable<int>
                where TDestination : class, IIdentifiable<int>
        {
            return CreateFrom(items, mappingFunction, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TSource, int> CreateFrom<TSource>(CursorPagedList<TSource, int> items, CursorPaginationParameters searchParams)
            where TSource : class, IIdentifiable<int>
        {
            return CreateFrom(items, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TSource, int> CreateFrom<TSource>(CursorPagedList<TSource, int> items, bool includeNodes = true, bool includeEdges = true)
            where TSource : class, IIdentifiable<int>
        {
            return CreateFrom<TSource, int>(
                items,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.TotalCount,
                Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
                includeNodes,
                includeEdges);
        }

        public static CursorPaginatedResponse<TDestination, long> CreateFrom<TSource, TDestination>(
            CursorPagedList<TSource, long> items,
            Func<IEnumerable<TSource>,
            IEnumerable<TDestination>> mappingFunction,
            CursorPaginationParameters searchParams)
                where TSource : class, IIdentifiable<long>
                where TDestination : class, IIdentifiable<long>
        {
            return CreateFrom(items, mappingFunction, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TDestination, long> CreateFrom<TSource, TDestination>(
            CursorPagedList<TSource, long> items,
            Func<IEnumerable<TSource>,
            IEnumerable<TDestination>> mappingFunction,
            bool includeNodes = true,
            bool includeEdges = true)
                where TSource : class, IIdentifiable<long>
                where TDestination : class, IIdentifiable<long>
        {
            var mappedItems = mappingFunction(items);

            return CreateFrom<TDestination, long>(
                mappedItems,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.TotalCount,
                Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
                includeNodes,
                includeEdges);
        }

        public static CursorPaginatedResponse<TSource, long> CreateFrom<TSource>(CursorPagedList<TSource, long> items, CursorPaginationParameters searchParams)
            where TSource : class, IIdentifiable<long>
        {
            return CreateFrom(items, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TSource, long> CreateFrom<TSource>(CursorPagedList<TSource, long> items, bool includeNodes = true, bool includeEdges = true)
            where TSource : class, IIdentifiable<long>
        {
            return CreateFrom<TSource, long>(
                items,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.TotalCount,
                Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
                includeNodes,
                includeEdges);
        }

        private static IEnumerable<Edge<TEntity>> GetEdges<TEntity, TEntityKey>(IEnumerable<TEntity> items, Func<TEntityKey, string> ConvertIdToBase64)
            where TEntity : class, IIdentifiable<TEntityKey>
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        {
            return items.Select(item => new Edge<TEntity>
            {
                Cursor = ConvertIdToBase64(item.Id),
                Node = item
            });
        }
    }
}