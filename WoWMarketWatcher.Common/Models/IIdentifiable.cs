using System;

namespace WoWMarketWatcher.Common.Models
{
    public interface IIdentifiable<TKey> where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        TKey Id { get; }
    }
}