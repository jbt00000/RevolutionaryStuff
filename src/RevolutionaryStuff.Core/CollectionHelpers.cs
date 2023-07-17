using System.Collections;
using System.Linq.Expressions;
using System.Text;
using RevolutionaryStuff.Core.Collections;

namespace RevolutionaryStuff.Core;

public static class CollectionHelpers
{
    public static void SetIfValNotNull<K, V>(this IDictionary<K, V> d, K key, V val) where V : class
    {
        if (val != null)
        {
            d[key] = val;
        }
    }

    public static void SetIfKeyNotFound<K, V>(this IDictionary<K, V> d, K key, V val)
    {
        if (!d.ContainsKey(key))
        {
            d[key] = val;
        }
    }

    public static IEnumerable<T> NullSafeEnumerable<T>(this IEnumerable<T> e)
    {
        return e ?? (new T[0]);
    }

    public static int NullSafeCount<T>(this IEnumerable<T> e)
    {
        return e == null ? 0 : e is IList l ? l.Count : e.Count();
    }

    public static bool NullSafeAny<T>(this IEnumerable<T> e, Func<T, bool> predicate = null)
        => e != null && (predicate == null ? e.Any() : e.Any(predicate));

    public static IList<KeyValuePair<string, string>> ToStringStringKeyValuePairs(this IEnumerable<KeyValuePair<string, object>> kvps)
    {
        var ret = new List<KeyValuePair<string, string>>();
        if (kvps != null)
        {
            foreach (var kvp in kvps)
            {
                ret.Add(new KeyValuePair<string, string>(kvp.Key, Stuff.ObjectToString(kvp.Value)));
            }
        }
        return ret;
    }

    public static IEnumerator GetEnumerator<K, V>(IEnumerable<KeyValuePair<K, V>> e)
    {
        foreach (var kvp in e)
        {
            yield return e;
        }
    }

    public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool isAscending)
    {
        return isAscending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
    }

    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool isAscending)
    {
        return isAscending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
    }

    public static void AddRange<T>(this HashSet<T> hs, IEnumerable<T> items)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                hs.Add(item);
            }
        }
    }

    public static IList<V> Map<TInput, K, V>(this IEnumerable<TInput> items, IDictionary<K, V> map, Func<TInput, K> keyGetter, bool omitMissing = false)
    {
        return items.Map(map, keyGetter, z => z, omitMissing);
    }

    public static IList<O> Map<TInput, K, V, O>(this IEnumerable<TInput> items, IDictionary<K, V> map, Func<TInput, K> keyGetter, Func<V, O> outputTransformer, bool omitMissing = false)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(keyGetter);
        ArgumentNullException.ThrowIfNull(outputTransformer);

        var ret = new List<O>();
        foreach (var item in items)
        {
            var key = keyGetter(item);
            if (map.TryGetValue(key, out var val))
            {
                ret.Add(outputTransformer(val));
            }
            else if (!omitMissing)
            {
                ret.Add(default);
            }
        }
        return ret;
    }

    public static IDictionary<TKey, TSource> ToDictionaryOnConflictKeepLast<TKey, TSource>(this IEnumerable<TSource> items, Func<TSource, TKey> keySelector)
        => items.ToDictionaryOnConflictKeepLast(keySelector, z => z);

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

    public static MultipleValueDictionary<TKey, TSource> ToMultipleValueDictionary<TKey, TSource>(this IEnumerable<TSource> items, Func<TSource, TKey> keySelector)
        => items.ToMultipleValueDictionary(keySelector, z => z);

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

    public static R FirstValueOfType<R>(this IDictionary<string, object> d)
    {
        if (d != null)
        {
            foreach (var v in d.Values)
            {
                if (v is R) return (R)v;
            }
        }
        return default;
    }

    /// <summary>
    /// Given a list of objects, return a random element
    /// </summary>
    /// <typeparam name="T">Type of argument in the list</typeparam>
    /// <param name="list">List of items</param>
    /// <param name="r">random number generator, null is ok</param>
    /// <returns>A random item from the list</returns>
    public static T Random<T>(this IList<T> list, Random r = null)
    {
        r ??= Stuff.Random;
        var n = r.Next(list.Count);
        return list[n];
    }

    /// <summary>
    /// Given a list of objects, randomizes the order
    /// </summary>
    /// <param name="list">List of items to be randomized in place</param>
    /// <param name="random">The random number generator to be used, when null, use the default</param>
    [Obsolete("Use Shuffle instead", false)]
    public static void ShuffleList(this IList list, Random random = null)
    {
        ArgumentNullException.ThrowIfNull(list);
        var len = list.Count;
        if (len < 2) return;
        random ??= Stuff.Random;
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

    public static void Shuffle<T>(this IList<T> list, Random random = null)
    {
        ArgumentNullException.ThrowIfNull(list);
        var len = list.Count;
        if (len < 2) return;
        random ??= Stuff.Random;
        int x, y;
        T o;
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
        return items == null ? (new T[0]) : new List<T>(items).AsReadOnly();
    }

    public static IDictionary<K, V> AsReadOnlyDictionary<K, V>(this IDictionary<K, V> dict)
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

    public static string Format(this IEnumerable e, string sep = "", string format = "{0}")
    {
        if (null == e) return "";
        var sb = new StringBuilder();
        var x = 0;
        foreach (var o in e)
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
        var x = 0;
        foreach (var o in e)
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
        if (e == null) return false;
        var z = e.GetEnumerator();
        return z.MoveNext();
    }

    public static V GetValue<K, V>(this IDictionary<K, V> d, K key, V fallback = default)
        => d.TryGetValue(key, out var ret) ? ret : fallback;

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
        if (items != null)
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

    public static IList<T> Remove<T>(this ICollection<T> col, Func<T, bool> toRemove)
    {
        if (col == null || toRemove == null) return Array.Empty<T>();
        List<T> removes = new();
        foreach (var item in col)
        {
            if (toRemove(item))
            {
                removes.Add(item);
            }
        }
        if (removes.Count > 0)
        {
            if (removes.Count == col.Count)
            {
                col.Clear();
            }
            else
            {
                col.Remove(removes);
            }
        }
        return removes;
    }

    public static int Remove<T>(this ICollection<T> col, IEnumerable<T> items)
    {
        if (col == null || items == null) return 0;
        var cnt = 0;
        foreach (var item in items)
        {
            cnt += col.Remove(item) ? 1 : 0;
        }
        return cnt;
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

    public static List<V> ToOrderedValuesList<K, V>(this IDictionary<K, V> d, IEnumerable<K> orderedKeys, bool throwOnMiss = false, V missingVal = default)
    {
        var ret = new List<V>(d.Count);
        foreach (var k in orderedKeys)
        {
            if (k != null && d.TryGetValue(k, out var v))
            {
                Stuff.Noop();
            }
            else
            {
                v = throwOnMiss ? throw new InvalidMappingException(k, "something in results dictionary") : missingVal;
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
            var z = 0;
            foreach (var o in e)
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
        if (d.TryGetValue(key, out var val))
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
        if (!d.TryGetValue(key, out var ret))
        {
            ret = missing;
        }
        return ret;
    }


    public static V FindOrDefault<K, V>(this IDictionary<K, V> d, K key)
    {
        if (!d.TryGetValue(key, out var ret))
        {
            ret = default;
        }
        return ret;
    }

    public static V FindOrCreate<K, V>(this IDictionary<K, V> d, K key, Func<V> creator)
    {
        if (!d.TryGetValue(key, out var ret))
        {
            ret = creator();
            d[key] = ret;
        }
        return ret;
    }

    public static V FindOrCreate<K, V>(this IDictionary<K, V> d, K key, Func<K, V> creator)
    {
        if (!d.TryGetValue(key, out var ret))
        {
            ret = creator(key);
            d[key] = ret;
        }
        return ret;
    }

    public static int? IndexOfOccurrence<T>(this IList<T> items, Func<T, bool> test, int nthOccurrence, int? zeroThValue = null, int? missingValue = null)
    {
        Requires.NonNegative(nthOccurrence);

        if (nthOccurrence == 0) return zeroThValue;

        var cnt = 0;
        for (var z = 0; z < items.Count; ++z)
        {
            var i = items[z];
            var hit = test(i);
            if (hit && ++cnt == nthOccurrence)
            {
                return z;
            }
        }
        return missingValue;
    }

    public static int? IndexOfOccurrence<T>(this IList<T> items, T match, int nthOccurrence, int? zeroThValue = null, int? missingValue = null)
       => items.IndexOfOccurrence(i =>
       {
           return i == null ? match == null : i.Equals(match);
       }, nthOccurrence, zeroThValue, missingValue);

    public static IList<KeyValuePair<string, string>> FluentAdd(this IList<KeyValuePair<string, string>> items, string key, string val, bool addIfNullOrWhiteSpace = true, bool addThis = true)
    {
        if (addThis)
        {
            if (addIfNullOrWhiteSpace || !string.IsNullOrWhiteSpace(val))
            {
                items.Add(new(key, val));
            }
        }
        return items;
    }

    public static TColl FluentAdd<TColl, T>(this TColl col, T item) where TColl : ICollection<T>
    {
        col.Add(item);
        return col;
    }

    public static IList<T> FluentAdd<T>(this IList<T> col, T item)
    {
        col.Add(item);
        return col;
    }

    public static TColl FluentAddRange<TColl, T>(this TColl col, IEnumerable<T> items) where TColl : ICollection<T>
    {
        if (items != null)
        {
            foreach (var i in items)
            {
                col.Add(i);
            }
        }
        return col;
    }

    public static IList<T> FluentClear<T>(this IList<T> col)
    {
        col.Clear();
        return col;
    }

    public static void ForEach<T>(this IEnumerable<T> col, Action<T, int> a)
    {
        ArgumentNullException.ThrowIfNull(col);
        ArgumentNullException.ThrowIfNull(a);

        var z = 0;
        foreach (var item in col)
        {
            a(item, z++);
        }
    }

    public static async Task<IList<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        ArgumentNullException.ThrowIfNull(asyncEnumerable);

        if (asyncEnumerable == null) return null;
        var items = new List<T>();
        await foreach (var item in asyncEnumerable)
        {
            items.Add(item);
        }
        return items;
    }

}
