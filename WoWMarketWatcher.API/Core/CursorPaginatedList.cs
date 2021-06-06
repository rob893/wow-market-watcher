using System;
using System.Collections.Generic;

namespace WoWMarketWatcher.API.Core
{
    public class CursorPaginatedList<TEntity, TEntityKey> : List<TEntity>, IList<TEntity>
        where TEntity : class
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string? StartCursor { get; set; }
        public string? EndCursor { get; set; }
        public int? TotalCount { get; set; }
        public int PageCount { get; set; }


        public CursorPaginatedList(ICollection<TEntity> items, bool hasNextPage, bool hasPreviousPage, string? startCursor, string? endCursor, int? totalCount)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.HasNextPage = hasNextPage;
            this.HasPreviousPage = hasPreviousPage;
            this.StartCursor = startCursor;
            this.EndCursor = endCursor;
            this.TotalCount = totalCount;
            this.PageCount = items.Count;
            this.AddRange(items);
        }
    }

    public class CursorPaginatedList<TEntity> : CursorPaginatedList<TEntity, int>
        where TEntity : class
    {
        public CursorPaginatedList(ICollection<TEntity> items, bool hasNextPage, bool hasPreviousPage, string? startCursor, string? endCursor, int? totalCount) :
            base(items, hasNextPage, hasPreviousPage, startCursor, endCursor, totalCount)
        { }
    }
}