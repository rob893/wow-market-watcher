using System;

namespace WoWMarketWatcher.API.Models
{
    public interface IIdentifiable<TKey> where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        TKey Id { get; }
    }
}