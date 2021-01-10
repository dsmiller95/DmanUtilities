using System;
using System.Collections.Generic;
using System.Linq;


namespace Dman.Utilities
{
    public static class LinqExtensions
    {
        /// <summary>
        /// For every <typeparamref name="T"/> in, if <paramref name="dictionary"/> has that key, emit value <typeparamref name="K"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="self"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static IEnumerable<K> TryPullFromDictionary<T, K>(this IEnumerable<T> self, IDictionary<T, K> dictionary)
        {
            foreach (var item in self)
            {
                if (dictionary.TryGetValue(item, out var value))
                {
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Partition <paramref name="source"/> into discrete sections of exactly <paramref name="bufferSize"/> length.
        ///     If the length of <paramref name="source"/> is not divisible by <paramref name="bufferSize"/>, some elements at the end
        ///     will never be emitted
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> Buffer<T>(this IEnumerable<T> source, int bufferSize)
        {
            if (bufferSize <= 0)
            {
                while (true)
                {
                    yield return new T[0];
                }
            }

            var iterator = source.GetEnumerator();

            int position;
            while (true)
            {
                var workingList = new T[bufferSize];
                for (position = 0; position < bufferSize && iterator.MoveNext(); position++)
                {
                    var value = iterator.Current;
                    workingList[position] = value;
                }
                if (position < bufferSize)
                {
                    yield break;
                }
                yield return workingList;
            }
        }

        /// <summary>
        /// Emit a new IList<typeparamref name="T"/> of length <paramref name="window"/> for every element in <paramref name="source"/>, except for the first
        ///     elements which are used to fill up the window initially
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static IEnumerable<IList<T>> RollingWindow<T>(this IEnumerable<T> source, int window)
        {
            var iterator = source.GetEnumerator();

            var position = 0;
            var workingList = new List<T>(window);
            while (position < window && iterator.MoveNext())
            {
                var value = iterator.Current;
                workingList.Add(value);
                position++;
            }
            if (position < window)
            {
                yield break;
            }

            yield return workingList.ToList();
            while (iterator.MoveNext())
            {
                var value = iterator.Current;
                workingList.RemoveAt(0);
                workingList.Add(value);
                yield return workingList.ToList();
            }
        }

        /// <summary>
        /// Return a new dictionary such that ever <typeparamref name="T"/> maps to the sum of every <typeparamref name="T"/> defined in
        ///     each dictionary in <paramref name="source"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDictionary<T, float> SumTogether<T>(this IEnumerable<IDictionary<T, float>> source)
        {
            var result = new Dictionary<T, float>();
            var iterator = source.GetEnumerator();

            while (iterator.MoveNext())
            {
                var value = iterator.Current;
                foreach (var key in value.Keys)
                {
                    if (!result.ContainsKey(key))
                    {
                        result[key] = 0f;
                    }
                    result[key] += value[key];
                }
            }

            return result;
        }

        /// <summary>
        /// shorthand for <see cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="source"></param>
        /// <param name="valueSelector"></param>
        /// <returns></returns>
        public static IDictionary<TKey, TOut> SelectDictionary<TKey, TIn, TOut>(this IDictionary<TKey, TIn> source, Func<TIn, TOut> valueSelector)
        {
            return source.ToDictionary(x => x.Key, x => valueSelector(x.Value));
        }

        public static IDictionary<TKey, float> Normalize<TKey>(this IDictionary<TKey, float> source)
        {
            var sum = source.Values.Sum();
            return source.SelectDictionary(f => f / sum);
        }
    }

}