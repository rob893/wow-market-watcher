using System;
using System.Collections.Generic;
using System.Linq;
using WoWMarketWatcher.API.Models;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses.Pagination;

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
            int pageCount,
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
                PageInfo = new CursorPaginatedResponsePageInfo
                {
                    StartCursor = startCursor,
                    EndCursor = endCursor,
                    HasNextPage = hasNextPage,
                    HasPreviousPage = hasPreviousPage,
                    PageCount = pageCount,
                    TotalCount = totalCount
                }
            };
        }

        public static CursorPaginatedResponse<TEntity, TEntityKey> CreateFrom<TEntity, TEntityKey>(
            CursorPaginatedList<TEntity, TEntityKey> items,
            Func<TEntityKey, string> ConvertIdToBase64,
            bool includeNodes = true,
            bool includeEdges = true)
                where TEntity : class, IIdentifiable<TEntityKey>
                where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return CreateFrom(
                items,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.PageCount,
                items.TotalCount,
                ConvertIdToBase64,
                includeNodes,
                includeEdges);
        }

        public static CursorPaginatedResponse<TDestination, int> CreateFrom<TSource, TDestination>(
            CursorPaginatedList<TSource, int> items, Func<IEnumerable<TSource>,
            IEnumerable<TDestination>> mappingFunction,
            bool includeNodes = true,
            bool includeEdges = true)
                where TSource : class, IIdentifiable<int>
                where TDestination : class, IIdentifiable<int>
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (mappingFunction == null)
            {
                throw new ArgumentNullException(nameof(mappingFunction));
            }

            var mappedItems = mappingFunction(items);

            return CreateFrom<TDestination, int>(
                mappedItems,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.PageCount,
                items.TotalCount,
                Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
                includeNodes,
                includeEdges);
        }

        public static CursorPaginatedResponse<TDestination, int> CreateFrom<TSource, TDestination>(
            CursorPaginatedList<TSource, int> items,
            Func<IEnumerable<TSource>, IEnumerable<TDestination>> mappingFunction,
            CursorPaginationQueryParameters searchParams)
                where TSource : class, IIdentifiable<int>
                where TDestination : class, IIdentifiable<int>
        {
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            return CreateFrom(items, mappingFunction, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TSource, int> CreateFrom<TSource>(CursorPaginatedList<TSource, int> items, CursorPaginationQueryParameters searchParams)
            where TSource : class, IIdentifiable<int>
        {
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            return CreateFrom(items, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TSource, int> CreateFrom<TSource>(CursorPaginatedList<TSource, int> items, bool includeNodes = true, bool includeEdges = true)
            where TSource : class, IIdentifiable<int>
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return CreateFrom<TSource, int>(
                items,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.PageCount,
                items.TotalCount,
                Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
                includeNodes,
                includeEdges);
        }

        public static CursorPaginatedResponse<TDestination, long> CreateFrom<TSource, TDestination>(
            CursorPaginatedList<TSource, long> items,
            Func<IEnumerable<TSource>,
            IEnumerable<TDestination>> mappingFunction,
            CursorPaginationQueryParameters searchParams)
                where TSource : class, IIdentifiable<long>
                where TDestination : class, IIdentifiable<long>
        {
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            return CreateFrom(items, mappingFunction, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TDestination, long> CreateFrom<TSource, TDestination>(
            CursorPaginatedList<TSource, long> items,
            Func<IEnumerable<TSource>,
            IEnumerable<TDestination>> mappingFunction,
            bool includeNodes = true,
            bool includeEdges = true)
                where TSource : class, IIdentifiable<long>
                where TDestination : class, IIdentifiable<long>
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (mappingFunction == null)
            {
                throw new ArgumentNullException(nameof(mappingFunction));
            }

            var mappedItems = mappingFunction(items);

            return CreateFrom<TDestination, long>(
                mappedItems,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.PageCount,
                items.TotalCount,
                Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
                includeNodes,
                includeEdges);
        }

        public static CursorPaginatedResponse<TSource, long> CreateFrom<TSource>(CursorPaginatedList<TSource, long> items, CursorPaginationQueryParameters searchParams)
            where TSource : class, IIdentifiable<long>
        {
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            return CreateFrom(items, searchParams.IncludeNodes, searchParams.IncludeEdges);
        }

        public static CursorPaginatedResponse<TSource, long> CreateFrom<TSource>(CursorPaginatedList<TSource, long> items, bool includeNodes = true, bool includeEdges = true)
            where TSource : class, IIdentifiable<long>
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return CreateFrom<TSource, long>(
                items,
                items.StartCursor,
                items.EndCursor,
                items.HasNextPage,
                items.HasPreviousPage,
                items.PageCount,
                items.TotalCount,
                Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
                includeNodes,
                includeEdges);
        }

        private static IEnumerable<CursorPaginatedResponseEdge<TEntity>> GetEdges<TEntity, TEntityKey>(IEnumerable<TEntity> items, Func<TEntityKey, string> ConvertIdToBase64)
            where TEntity : class, IIdentifiable<TEntityKey>
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return items.Select(item => new CursorPaginatedResponseEdge<TEntity>
            {
                Cursor = ConvertIdToBase64(item.Id),
                Node = item
            });
        }
    }
}