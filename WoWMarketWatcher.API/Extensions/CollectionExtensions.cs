using System;
using System.Collections.Generic;
using System.Linq;

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

        public static long Percentile(this IEnumerable<long> source, double percentile)
        {
            if (percentile < 0 || percentile > 1)
            {
                throw new ArgumentException($"{nameof(percentile)} must be >= 0 and <= 1");
            }

            var elements = source.ToArray();
            Array.Sort(elements);
            var index = (int)Math.Floor(percentile * (elements.Length - 1));

            return elements[index];
        }
    }
}