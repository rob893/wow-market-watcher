using System;

namespace WoWMarketWatcher.Common.Models
{
    public interface IOwnedByUser<TKey> where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        TKey UserId { get; }
    }
}