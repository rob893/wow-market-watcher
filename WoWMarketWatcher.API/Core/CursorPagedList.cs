using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Core
{
    public class CursorPagedList<TEntity, TEntityKey> : List<TEntity>
        where TEntity : class, IIdentifiable<TEntityKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string? StartCursor { get; set; }
        public string? EndCursor { get; set; }
        public int? TotalCount { get; set; }


        public CursorPagedList(IEnumerable<TEntity> items, bool hasNextPage, bool hasPreviousPage, string? startCursor, string? endCursor, int? totalCount)
        {
            HasNextPage = hasNextPage;
            HasPreviousPage = hasPreviousPage;
            StartCursor = startCursor;
            EndCursor = endCursor;
            TotalCount = totalCount;
            AddRange(items);
        }

        public static async Task<CursorPagedList<T, R>> CreateAsync<T, R>(IQueryable<T> source, int? first, string? after, int? last, string? before, bool includeTotal,
        Func<R, string> ConvertIdToBase64, Func<string, R> ConvertBase64ToIdType, Func<IQueryable<T>, R, IQueryable<T>> AddAfterExp, Func<IQueryable<T>, R, IQueryable<T>> AddBeforeExp)
            where T : class, IIdentifiable<R>
            where R : IEquatable<R>, IComparable<R>
        {
            if (first != null && last != null)
            {
                throw new ArgumentException("Passing both `first` and `last` to paginate is not supported.");
            }

            int? totalCount = null;

            if (includeTotal)
            {
                totalCount = await source.CountAsync();
            }

            if (first == null && last == null)
            {
                var items = await source.OrderBy(item => item.Id).ToListAsync();

                var firstItem = items.FirstOrDefault();
                var lastItem = items.LastOrDefault();

                var startCursor = firstItem != null ? ConvertIdToBase64(firstItem.Id) : null;
                var endCursor = lastItem != null ? ConvertIdToBase64(lastItem.Id) : null;

                return new CursorPagedList<T, R>(items, false, false, startCursor, endCursor, totalCount);
            }

            if (first != null)
            {
                if (first.Value < 0)
                {
                    throw new ArgumentException("first cannot be less than 0.");
                }

                if (after != null)
                {
                    var afterId = ConvertBase64ToIdType(after);
                    source = AddAfterExp(source, afterId);
                }

                if (before != null)
                {
                    var beforeId = ConvertBase64ToIdType(before);
                    source = AddBeforeExp(source, beforeId);
                }

                var items = await source.OrderBy(item => item.Id).Take(first.Value + 1).ToListAsync();

                var hasNextPage = items.Count >= first.Value + 1;
                var hasPreviousPage = after != null;

                if (items.Count >= first.Value + 1)
                {
                    items.RemoveAt(items.Count - 1);
                }

                var firstItem = items.FirstOrDefault();
                var lastItem = items.LastOrDefault();

                var startCursor = firstItem != null ? ConvertIdToBase64(firstItem.Id) : null;
                var endCursor = lastItem != null ? ConvertIdToBase64(lastItem.Id) : null;

                return new CursorPagedList<T, R>(items, hasNextPage, hasPreviousPage, startCursor, endCursor, totalCount);
            }

            if (last != null)
            {
                if (last.Value < 0)
                {
                    throw new ArgumentException("last cannot be less than 0.");
                }

                if (after != null)
                {
                    var afterId = ConvertBase64ToIdType(after);
                    source = AddAfterExp(source, afterId);
                }

                if (before != null)
                {
                    var beforeId = ConvertBase64ToIdType(before);
                    source = AddBeforeExp(source, beforeId);
                }

                var items = await source.OrderByDescending(item => item.Id).Take(last.Value + 1).ToListAsync();

                var hasNextPage = before != null;
                var hasPreviousPage = items.Count >= last.Value + 1;

                if (items.Count >= last.Value + 1)
                {
                    items.RemoveAt(items.Count - 1);
                }

                items.Reverse();

                var firstItem = items.FirstOrDefault();
                var lastItem = items.LastOrDefault();

                var startCursor = firstItem != null ? ConvertIdToBase64(firstItem.Id) : null;
                var endCursor = lastItem != null ? ConvertIdToBase64(lastItem.Id) : null;

                return new CursorPagedList<T, R>(items, hasNextPage, hasPreviousPage, startCursor, endCursor, totalCount);
            }

            throw new Exception("Error creating cursor paged list.");
        }

        public static Task<CursorPagedList<T, int>> CreateAsync<T>(IQueryable<T> source, int? first, string? after, int? last, string? before, bool includeTotal = false)
            where T : class, IIdentifiable<int>
        {
            return CreateAsync(source, first, after, last, before, includeTotal,
                Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
                str =>
                {
                    try
                    {
                        return BitConverter.ToInt32(Convert.FromBase64String(str), 0);
                    }
                    catch
                    {
                        throw new ArgumentException($"{str} is not a valid base 64 encoded int32.");
                    }
                },
                (source, afterId) => source.Where(item => item.Id > afterId),
                (source, beforeId) => source.Where(item => item.Id < beforeId)
            );
        }

        public static Task<CursorPagedList<T, int>> CreateAsync<T>(IQueryable<T> source, CursorPaginationParameters searchParams)
            where T : class, IIdentifiable<int>
        {
            return CreateAsync(source, searchParams.First, searchParams.After, searchParams.Last, searchParams.Before, searchParams.IncludeTotal);
        }

        public static Task<CursorPagedList<T, R>> CreateAsync<T, R>(IQueryable<T> source, CursorPaginationParameters searchParams,
            Func<R, string> ConvertIdToBase64, Func<string, R> ConvertBase64ToIdType, Func<IQueryable<T>, R, IQueryable<T>> AddAfterExp, Func<IQueryable<T>, R, IQueryable<T>> AddBeforeExp)
            where T : class, IIdentifiable<R>
            where R : IEquatable<R>, IComparable<R>
        {
            return CreateAsync(source, searchParams.First, searchParams.After, searchParams.Last, searchParams.Before, searchParams.IncludeTotal, ConvertIdToBase64, ConvertBase64ToIdType, AddAfterExp, AddBeforeExp);
        }
    }

    public class CursorPagedList<TEntity> : CursorPagedList<TEntity, int>
        where TEntity : class, IIdentifiable<int>
    {
        public CursorPagedList(IEnumerable<TEntity> items, bool hasNextPage, bool hasPreviousPage, string? startCursor, string? endCursor, int? totalCount) : base(
            items,
            hasNextPage,
            hasPreviousPage,
            startCursor,
            endCursor,
            totalCount
        )
        { }
    }
}