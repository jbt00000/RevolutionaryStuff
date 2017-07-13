using RevolutionaryStuff.Core.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RevolutionaryStuff.Core
{
    public static class CollectionHelpers
    {
        public static IEnumerator GetEnumerator<K, V>(IEnumerable<KeyValuePair<K, V>> e)
        {
            foreach (var kvp in e)
            {
                yield return e;
            }
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool isAscending)
        {
            if (isAscending)
            {
                return source.OrderBy(keySelector);
            }
            else
            {
                return source.OrderByDescending(keySelector);
            }
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool isAscending)
        {
            if (isAscending)
            {
                return source.OrderBy(keySelector);
            }
            else
            {
                return source.OrderByDescending(keySelector);
            }
        }

        public static IList<V> Map<TInput, K, V>(this IEnumerable<TInput> items, IDictionary<K, V> map, Func<TInput, K> keyGetter, bool omitMissing = false)
        {
            return items.Map(map, keyGetter, z => z, omitMissing);
        }

        public static IList<O> Map<TInput, K, V, O>(this IEnumerable<TInput> items, IDictionary<K, V> map, Func<TInput, K> keyGetter, Func<V, O> outputTransformer, bool omitMissing = false)
        {
            Requires.NonNull(map, nameof(map));
            Requires.NonNull(keyGetter, nameof(keyGetter));
            Requires.NonNull(outputTransformer, nameof(outputTransformer));

            var ret = new List<O>();
            foreach (var item in items)
            {
                var key = keyGetter(item);
                if (map.TryGetValue(key, out V val))
                {
                    ret.Add(outputTransformer(val));
                }
                else if (!omitMissing)
                {
                    ret.Add(default(O));
                }
            }
            return ret;
        }

        public static IDictionary<TKey, TVal> ToDictionaryOnConflictKeepLast<TKey, TVal, TSource>(this IEnumerable<TSource> items, Func<TSource, TKey> keySelector, Func<TSource, TVal> valSelector)
        {
            var d = new Dictionary<TKey, TVal>();
            if (items != null)
            {
                foreach (var item in items)
                {
                    d[keySelector(item)] = valSelector(item);
                }
            }
            return d;
        }

        public static MultipleValueDictionary<TKey, TVal> ToMultipleValueDictionary<TKey, TVal, TSource>(this IEnumerable<TSource> items, Func<TSource, TKey> keySelector, Func<TSource, TVal> valSelector)
        {
            var m = new MultipleValueDictionary<TKey, TVal>();
            if (items != null)
            {
                foreach (var item in items)
                {
                    m.Add(keySelector(item), valSelector(item));
                }
            }
            return m;
        }

        public static MultipleValueDictionary<TKey, TSource> ToMultipleValueDictionary<TKey, TSource>(this IEnumerable<TSource> items, Func<TSource, TKey> keySelector)
        {
            return items.ToMultipleValueDictionary(keySelector, z => z);
        }

        public static R FirstValueOfType<R>(this IDictionary<string, object> d)
        {
            if (d != null)
            {
                foreach (object v in d.Values)
                {
                    if (v is R) return (R)v;
                }
            }
            return default(R);
        }

        /// <summary>
        /// Given a list of objects, randomizes the order
        /// </summary>
        /// <param name="list">List of items to be randomized in place</param>
        /// <param name="random">The random number generator to be used, when null, use the default</param>
        public static void ShuffleList(this IList list, Random random = null)
        {
            Requires.NonNull(list, nameof(list));
            int len = list.Count;
            if (len < 2) return;
            if (null == random)
            {
                random = Stuff.Random;
            }
            int x, y;
            object o;
            for (x = 0; x < len; ++x)
            {
                y = random.Next(len);
                o = list[x];
                list[x] = list[y];
                list[y] = o;
            }
        }

        public static IList<T> AsReadOnly<T>(this IEnumerable<T> items)
        {
            if (items == null) return new T[0];
            return new List<T>(items).AsReadOnly();
        }

        public static IDictionary<K, V> AsReadOnly<K, V>(this IDictionary<K, V> dict)
        {
            return new ReadonlyDictionary<K, V>(dict);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> items) where T : class
        {
            return items.Where(i => i != null);
        }

        public static void AddFormat(this ICollection<string> col, string format, params object[] args)
        {
            var msg = string.Format(format, args);
            col.Add(msg);
        }

        public static void AddFormat<K>(this MultipleValueDictionary<K, string> m, K key, string format, params object[] args)
        {
            var msg = string.Format(format, args);
            m.Add(key, msg);
        }

        public static string Format(this IEnumerable e, string sep="", string format="{0}")
        {
            if (null == e) return "";
            var sb = new StringBuilder();
            int x = 0;
            foreach (object o in e)
            {
                if (x > 0 && null != sep)
                {
                    sb.Append(sep);
                }
                sb.AppendFormat(format, o, x++);
            }
            return sb.ToString();
        }

        public static string Format<T>(this IEnumerable<T> e, string sep, Func<T, int, string> formatter)
        {
            if (null == e) return "";
            var sb = new StringBuilder();
            int x = 0;
            foreach (T o in e)
            {
                if (x > 0 && null != sep)
                {
                    sb.Append(sep);
                }
                sb.Append(formatter(o, x++));
            }
            return sb.ToString();
        }

        public static bool HasData(this IEnumerable e)
        {
            var z = e.GetEnumerator();
            return z.MoveNext();
        }

        public static V GetValue<K, V>(this IDictionary<K, V> d, K key, V fallback = default(V))
            => d.TryGetValue(key, out V ret) ? ret : fallback;

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items != null)
            {
                foreach (var d in items) { action(d); }
            }
            return items;
        }

        public static HashSet<T> ToSet<T>(this IEnumerable<T> items)
        {
            return items.ToSet(i => i);
        }

        public static HashSet<TOut> ToSet<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> converter)
        {
            var set = new HashSet<TOut>();
            if (items != null)
            {
                foreach (var i in items)
                {
                    set.Add(converter(i));
                }
            }
            return set;
        }

        public static HashSet<string> ToCaseInsensitiveSet(this IEnumerable<string> items)
        {
            var set = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            if (items!=null)
            {
                foreach (var s in items)
                {
                    set.Add(s);
                }
            }
            return set;
        }

        public static List<TOutput> ConvertAll<T, TOutput>(this IEnumerable<T> datas, Func<T, TOutput> converter)
        {
            var ret = new List<TOutput>();
            if (datas != null)
            {
                foreach (var data in datas)
                {
                    ret.Add(converter(data));
                }
            }
            return ret;
        }

        public static void Remove<T>(this ICollection<T> col, IEnumerable<T> items)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                col.Remove(item);
            }
        }

        public static IList<T> OrderBy<T>(this IEnumerable<T> items) where T : IComparable
        {
            var l = new List<T>(items);
            l.Sort();
            return l;
        }

        public static bool ContainsAnyElement<T>(this HashSet<T> set, IEnumerable<T> other)
        {
            if (other != null)
            {
                foreach (var z in other)
                {
                    if (set.Contains(z)) return true;
                }
            }
            return false;
        }

        public static List<V> ToOrderedValuesList<K, V>(this IDictionary<K, V> d, IEnumerable<K> orderedKeys, bool throwOnMiss=false, V missingVal = default(V))
        {
            var ret = new List<V>(d.Count);
            foreach (var k in orderedKeys)
            {
                if (k != null && d.TryGetValue(k, out V v))
                {
                    Stuff.Noop();
                }
                else if (throwOnMiss)
                {
                    throw new InvalidMappingException(k, "something in results dictionary");
                }
                else
                {
                    v = missingVal;
                }
                ret.Add(v);
            }
            return ret;
        }

        /// <summary>
        /// Gets an item out of a collection.  The item selected is random.
        /// Primary used when a collection has exactly 1 item.
        /// </summary>
        /// <param name="e">The enumerable in which the item exists</param>
        /// <param name="itemNum">The item # you want to fetch</param>
        /// <param name="missing">The object to return if the enumeralbe is empty</param>
        /// <returns>The chosen item</returns>
        internal static object GetItem(IEnumerable e, int itemNum, object missing)
        {
            if (e != null)
            {
                int z = 0;
                foreach (object o in e)
                {
                    if (z++ == itemNum)
                    {
                        return o;
                    }
                }
            }
            return missing;
        }

        public static IList<IList<T>> Chunkify<T>(this IEnumerable<T> items, int sublistSize)
        {
            var ret = new List<IList<T>>();
            List<T> sub = null;
            foreach (var i in items)
            {
                if (sub == null)
                {
                    sub = new List<T>(sublistSize);
                    ret.Add(sub);
                }
                sub.Add(i);
                if (sub.Count == sublistSize)
                {
                    sub = null;
                }
            }
            return ret;
        }

        public static int Increment<K>(this IDictionary<K, int> d, K key)
        {
            return d.Increment(key, 1);
        }

        public static int Increment<K>(this IDictionary<K, int> d, K key, int incrementAmount)
        {
            return d.Increment(key, incrementAmount, incrementAmount);
        }

        public static int Increment<K>(this IDictionary<K, int> d, K key, int incrementAmount, int initialAmount)
        {
            if (d.TryGetValue(key, out int val))
            {
                val += incrementAmount;
            }
            else
            {
                val = initialAmount;
            }
            d[key] = val;
            return val;
        }

        public static V FindOrMissing<K, V>(this IDictionary<K, V> d, K key, V missing)
        {
            if (!d.TryGetValue(key, out V ret))
            {
                ret = missing;
            }
            return ret;
        }


        public static V FindOrDefault<K, V>(this IDictionary<K, V> d, K key)
        {
            if (!d.TryGetValue(key, out V ret))
            {
                ret = default(V);
            }
            return ret;
        }

        public static V FindOrCreate<K, V>(this IDictionary<K, V> d, K key, Func<V> creator)
        {
            if (!d.TryGetValue(key, out V ret))
            {
                ret = creator();
                d[key] = ret;
            }
            return ret;
        }

        public static V FindOrCreate<K, V>(this IDictionary<K, V> d, K key, Func<K, V> creator)
        {
            if (!d.TryGetValue(key, out V ret))
            {
                ret = creator(key);
                d[key] = ret;
            }
            return ret;
        }
    }
}
