using System;

namespace WoWMarketWatcher.API.Models
{
    public interface IOwnedByUser<TKey> where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        TKey UserId { get; }
    }
}