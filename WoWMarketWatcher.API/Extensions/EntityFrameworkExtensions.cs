using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Models;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Extensions
{
    public static class EntityFrameworkExtensions
    {
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
        {
            if (dbSet == null)
            {
                throw new ArgumentNullException(nameof(dbSet));
            }

            dbSet.RemoveRange(dbSet);
        }

        public static async Task<CursorPaginatedList<TEntity, TEntityKey>> ToCursorPaginatedListAsync<TEntity, TEntityKey>(
            this IQueryable<TEntity> src,
            Expression<Func<TEntity, TEntityKey>> keySelector,
            Func<TEntityKey, string> keyConverter,
            Func<string, TEntityKey> cursorConverter,
            int? first,
            int? last,
            string? afterCursor,
            string? beforeCursor,
            bool includeTotal)
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

            if (first != null && last != null)
            {
                throw new NotSupportedException($"Passing both `{nameof(first)}` and `{nameof(last)}` to paginate is not supported.");
            }

            if (afterCursor != null)
            {
                var after = cursorConverter(afterCursor);
                src = src.Where(keySelector.Apply(key => key.CompareTo(after) > 0));
            }

            if (beforeCursor != null)
            {
                var before = cursorConverter(beforeCursor);
                src = src.Where(keySelector.Apply(key => key.CompareTo(before) < 0));
            }

            var pageList = new List<TEntity>();
            var hasNextPage = beforeCursor != null;
            var hasPreviousPage = afterCursor != null;

            if (first != null)
            {
                if (first.Value < 0)
                {
                    throw new ArgumentException($"{nameof(first)} cannot be less than 0.", nameof(first));
                }

                pageList = await src.OrderBy(keySelector).Take(first.Value + 1).ToListAsync();

                hasNextPage = pageList.Count > first.Value;

                if (hasNextPage)
                {
                    pageList.RemoveAt(pageList.Count - 1);
                }
            }
            else if (last != null)
            {
                if (last.Value < 0)
                {
                    throw new ArgumentException($"{nameof(last)} cannot be less than 0.", nameof(last));
                }

                pageList = await src.OrderByDescending(keySelector).Take(last.Value + 1).ToListAsync();

                hasPreviousPage = pageList.Count > last.Value;

                if (hasPreviousPage)
                {
                    pageList.RemoveAt(pageList.Count - 1);
                }

                pageList.Reverse();
            }
            else
            {
                pageList = await src.OrderBy(keySelector).ToListAsync();
            }

            var firstPageItem = pageList.FirstOrDefault();
            var lastPageItem = pageList.LastOrDefault();

            var keySelectorCompiled = keySelector.Compile();

            return new CursorPaginatedList<TEntity, TEntityKey>(
                pageList,
                hasNextPage,
                hasPreviousPage,
                firstPageItem != null ? keyConverter(keySelectorCompiled(firstPageItem)) : null,
                lastPageItem != null ? keyConverter(keySelectorCompiled(lastPageItem)) : null,
                includeTotal ? await src.CountAsync() : null);
        }

        public static Task<CursorPaginatedList<TEntity, TEntityKey>> ToCursorPaginatedListAsync<TEntity, TEntityKey>(
            this IQueryable<TEntity> src,
            Expression<Func<TEntity, TEntityKey>> keySelector,
            Func<TEntityKey, string> keyConverter,
            Func<string, TEntityKey> cursorConverter,
            CursorPaginationQueryParameters queryParameters)
                where TEntity : class
                where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        {
            if (queryParameters == null)
            {
                throw new ArgumentNullException(nameof(queryParameters));
            }

            return src.ToCursorPaginatedListAsync(
                keySelector,
                keyConverter,
                cursorConverter,
                queryParameters.First,
                queryParameters.Last,
                queryParameters.After,
                queryParameters.Before,
                queryParameters.IncludeTotal);
        }

        public static Task<CursorPaginatedList<TEntity, int>> ToCursorPaginatedListAsync<TEntity>(
            this IQueryable<TEntity> src,
            CursorPaginationQueryParameters queryParameters)
                where TEntity : class, IIdentifiable<int>
        {
            if (queryParameters == null)
            {
                throw new ArgumentNullException(nameof(queryParameters));
            }

            return src.ToCursorPaginatedListAsync(
                item => item.Id,
                key => key.ConvertToBase64Url(),
                cursor => cursor.ConvertToInt32FromBase64Url(),
                queryParameters.First,
                queryParameters.Last,
                queryParameters.After,
                queryParameters.Before,
                queryParameters.IncludeTotal);
        }
    }
}