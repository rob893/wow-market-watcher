using System;
using System.Collections;
using System.Collections.Generic;

namespace WoWMarketWatcher.API.Core
{
    public sealed class CursorPaginatedList<TEntity, TEntityKey> : IEnumerable<TEntity>
        where TEntity : class
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        private readonly List<TEntity> items;

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
            this.items = new List<TEntity>(items);
        }

        public bool HasNextPage { get; }

        public bool HasPreviousPage { get; }

        public string? StartCursor { get; }

        public string? EndCursor { get; }

        public int? TotalCount { get; }

        public int PageCount { get; }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }
    }
}