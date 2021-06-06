using System;
using System.Collections.Generic;
using System.Linq;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses.Pagination;

namespace WoWMarketWatcher.API.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => (Index: i, Value: x))
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList());
        }

        public static long Percentile(this IEnumerable<long> source, double percentile, bool isSourceSorted = false)
        {
            if (percentile < 0 || percentile > 1)
            {
                throw new ArgumentException($"{nameof(percentile)} must be >= 0 and <= 1");
            }

            if (source is long[] asArray)
            {
                var index = (int)Math.Floor(percentile * (asArray.Length - 1));

                if (isSourceSorted)
                {
                    return asArray[index];
                }

                var copy = source.ToArray();
                Array.Sort(copy);

                return copy[index];
            }

            var copyAsArray = source.ToArray();
            var i = (int)Math.Floor(percentile * (copyAsArray.Length - 1));

            Array.Sort(copyAsArray);

            return copyAsArray[i];
        }

        /// <summary>
        /// Converts an IEnumerable to a CursorPaginatedResponse.
        /// </summary>
        /// <param name="src">The collection.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="keyConverter">The key converter.</param>
        /// <param name="cursorConverter">The cursor converter.</param>
        /// <param name="queryParameters">The query parameters.</param>
        /// <typeparam name="TEntity">Type of entity.</typeparam>
        /// <typeparam name="TEntityKey">Type of entity key.</typeparam>
        /// <returns>The CursorPaginatedResponse.</returns>
        public static CursorPaginatedResponse<TEntity, TEntityKey> ToCursorPaginatedResponse<TEntity, TEntityKey>(
            this IEnumerable<TEntity> src,
            Func<TEntity, TEntityKey> keySelector,
            Func<TEntityKey, string> keyConverter,
            Func<string, TEntityKey> cursorConverter,
            CursorPaginationQueryParameters queryParameters)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (keyConverter == null)
            {
                throw new ArgumentNullException(nameof(keyConverter));
            }

            if (cursorConverter == null)
            {
                throw new ArgumentNullException(nameof(cursorConverter));
            }

            if (queryParameters == null)
            {
                throw new ArgumentNullException(nameof(queryParameters));
            }

            if (queryParameters.First != null && queryParameters.Last != null)
            {
                throw new NotSupportedException($"Passing both `{nameof(queryParameters.First)}` and `{nameof(queryParameters.Last)}` to paginate is not supported.");
            }

            var srcList = src.ToList();

            var orderedItems = srcList.OrderBy(keySelector).AsEnumerable();

            if (queryParameters.After != null)
            {
                var after = cursorConverter(queryParameters.After);
                orderedItems = orderedItems.Where(item => keySelector(item).CompareTo(after) > 0);
            }

            if (queryParameters.Before != null)
            {
                var before = cursorConverter(queryParameters.Before);
                orderedItems = orderedItems.Where(item => keySelector(item).CompareTo(before) < 0);
            }

            if (queryParameters.First != null)
            {
                if (queryParameters.First.Value < 0)
                {
                    throw new ArgumentException($"{nameof(queryParameters.First)} cannot be less than 0.", nameof(queryParameters));
                }

                orderedItems = orderedItems.Take(queryParameters.First.Value);
            }
            else if (queryParameters.Last != null)
            {
                if (queryParameters.Last.Value < 0)
                {
                    throw new ArgumentException($"{nameof(queryParameters.Last)} cannot be less than 0.", nameof(queryParameters));
                }

                orderedItems = orderedItems.TakeLast(queryParameters.Last.Value);
            }

            var pageList = orderedItems.Select(item => new CursorPaginatedResponseEdge<TEntity>
            {
                Cursor = keyConverter(keySelector(item)),
                Node = item
            }).ToList();

            var firstPageItem = pageList.FirstOrDefault();
            var lastPageItem = pageList.LastOrDefault();

            var firstSrcItem = srcList.FirstOrDefault();
            var lastSrcItem = srcList.LastOrDefault();

            return new CursorPaginatedResponse<TEntity, TEntityKey>
            {
                Edges = queryParameters.IncludeEdges ? pageList : null,
                Nodes = queryParameters.IncludeNodes ? orderedItems : null,
                PageInfo = new CursorPaginatedResponsePageInfo
                {
                    StartCursor = firstPageItem?.Cursor,
                    EndCursor = lastPageItem?.Cursor,
                    HasNextPage = lastPageItem != null && lastSrcItem != null && keySelector(lastSrcItem).CompareTo(keySelector(lastPageItem.Node)) > 0,
                    HasPreviousPage = firstPageItem != null && firstSrcItem != null && keySelector(firstSrcItem).CompareTo(keySelector(firstPageItem.Node)) < 0,
                    PageCount = pageList.Count,
                    TotalCount = queryParameters.IncludeTotal ? srcList.Count : null
                }
            };
        }

        /// <summary>
        /// Converts an IEnumerable to a CursorPaginatedResponse.
        /// </summary>
        /// <param name="src">The collection.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="queryParameters">The query parameters.</param>
        /// <typeparam name="TEntity">Type of entity.</typeparam>
        /// <returns>The CursorPaginatedResponse.</returns>
        public static CursorPaginatedResponse<TEntity, int> ToCursorPaginatedResponse<TEntity>(
            this IEnumerable<TEntity> src,
            Func<TEntity, int> keySelector,
            CursorPaginationQueryParameters queryParameters)
            where TEntity : class
        {
            return src.ToCursorPaginatedResponse(
                keySelector,
                key => key.ConvertToBase64(),
                cursor => cursor.ConvertToInt32FromBase64(),
                queryParameters);
        }

        /// <summary>
        /// Converts an IEnumerable to a CursorPaginatedResponse.
        /// </summary>
        /// <param name="src">The collection.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="queryParameters">The query parameters.</param>
        /// <typeparam name="TEntity">Type of entity.</typeparam>
        /// <returns>The CursorPaginatedResponse.</returns>
        public static CursorPaginatedResponse<TEntity, string> ToCursorPaginatedResponse<TEntity>(
            this IEnumerable<TEntity> src,
            Func<TEntity, string> keySelector,
            CursorPaginationQueryParameters queryParameters)
            where TEntity : class
        {
            return src.ToCursorPaginatedResponse(
                keySelector,
                key => key.ConvertToBase64(),
                cursor => cursor.ConvertToStringFromBase64(),
                queryParameters);
        }
    }
}