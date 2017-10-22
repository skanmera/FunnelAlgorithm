using System;
using System.Collections.Generic;
using System.Linq;

namespace FunnelAlgorithm
{
    public static class EnumerableExtension
    {
        public static IEnumerable<Pair<TSource>> MakePairs<TSource>(
            this IEnumerable<TSource> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    var previous = enumerator.Current;
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        yield return new Pair<TSource>(previous, current);

                        previous = current;
                    }
                }
            }
        }

        public static TSource FindMin<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.ToList().Find(s => selector(s).Equals(source.Min(selector)));
        }

        public static IEnumerable<IEnumerable<T>> SplitByEquality<T>(
            this IEnumerable<T> source)
        {
            return source.SplitByRegularity((items, current) => items.Last().Equals(current));
        }

        public static IEnumerable<IEnumerable<T>> SplitByRegularity<T>(
            this IEnumerable<T> source, Func<List<T>, T, bool> predicate)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    yield break;

                var items = new List<T> { enumerator.Current };
                while (enumerator.MoveNext())
                {
                    if (predicate(items, enumerator.Current))
                    {
                        items.Add(enumerator.Current);
                        continue;
                    }

                    yield return items;

                    items = new List<T> { enumerator.Current };
                }

                if (items.Any())
                    yield return items;
            }
        }
    }
}
