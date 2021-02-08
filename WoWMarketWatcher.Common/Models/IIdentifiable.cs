using System;

namespace WoWMarketWatcher.API.Entities
{
    public interface IIdentifiable<TKey> where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        TKey Id { get; }
    }
}