using System.Collections;
using System.Linq.Expressions;
using System.Text;
using RevolutionaryStuff.Core.Collections;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides utility methods for working with collections, including dictionaries, lists, sets, and enumerables.
/// Includes operations for manipulation, transformation, querying, and formatting collections.
/// </summary>
public static class CollectionHelpers
{
    /// <summary>
    /// Sets a key-value pair in the dictionary only if the value is not null.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary (must be a reference type).</typeparam>
    /// <param name="d">The dictionary to update.</param>
    /// <param name="key">The key to set.</param>
    /// <param name="val">The value to set. If null, no operation is performed.</param>
    public static void SetIfValNotNull<K, V>(this IDictionary<K, V> d, K key, V val) where V : class
    {
        if (val != null)
        {
            d[key] = val;
        }
    }

    /// <summary>
    /// Sets a key-value pair in the dictionary only if the key does not already exist.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    /// <param name="d">The dictionary to update.</param>
    /// <param name="key">The key to set.</param>
    /// <param name="val">The value to set.</param>
    public static void SetIfKeyNotFound<K, V>(this IDictionary<K, V> d, K key, V val)
    {
        if (!d.ContainsKey(key))
        {
            d[key] = val;
        }
    }

    /// <summary>
    /// Returns a null-safe enumerable, providing an empty collection if the input is null.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="e">The enumerable to check.</param>
    /// <returns>The original enumerable if not null; otherwise, an empty array.</returns>
    public static IEnumerable<T> NullSafeEnumerable<T>(this IEnumerable<T> e)
    {
        return e ?? (new T[0]);
    }

    /// <summary>
    /// Returns the count of elements in an enumerable, handling null safely by returning 0.
    /// Optimized to use IList.Count when available.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="e">The enumerable to count.</param>
    /// <returns>The number of elements, or 0 if the enumerable is null.</returns>
    public static int NullSafeCount<T>(this IEnumerable<T> e)
    {
        return e == null ? 0 : e is IList l ? l.Count : e.Count();
    }

    /// <summary>
    /// Determines whether an enumerable contains any elements, with optional predicate filtering.
    /// Returns false for null enumerables.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="e">The enumerable to check.</param>
    /// <param name="predicate">Optional predicate to filter elements.</param>
    /// <returns><c>true</c> if the enumerable is not null and contains matching elements; otherwise, <c>false</c>.</returns>
    public static bool NullSafeAny<T>(this IEnumerable<T> e, Func<T, bool> predicate = null)
        => e != null && (predicate == null ? e.Any() : e.Any(predicate));

    /// <summary>
    /// Converts a collection of string-object key-value pairs to string-string key-value pairs.
    /// </summary>
    /// <param name="kvps">The collection of key-value pairs to convert.</param>
    /// <returns>A list of string-string key-value pairs.</returns>
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

    /// <summary>
    /// Gets an enumerator for a collection of key-value pairs.
    /// </summary>
    /// <typeparam name="K">The type of keys.</typeparam>
    /// <typeparam name="V">The type of values.</typeparam>
    /// <param name="e">The enumerable of key-value pairs.</param>
    /// <returns>An enumerator for the collection.</returns>
    public static IEnumerator GetEnumerator<K, V>(IEnumerable<KeyValuePair<K, V>> e)
    {
        foreach (var kvp in e)
        {
            yield return e;
        }
    }

    /// <summary>
    /// Orders a queryable collection by a key selector in ascending or descending order.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="source">The queryable collection to order.</param>
    /// <param name="keySelector">Expression to select the ordering key.</param>
    /// <param name="isAscending">If <c>true</c>, orders ascending; otherwise, descending.</param>
    /// <returns>An ordered queryable collection.</returns>
    public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool isAscending)
    {
        return isAscending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
    }

    /// <summary>
    /// Orders an enumerable collection by a key selector in ascending or descending order.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="source">The enumerable collection to order.</param>
    /// <param name="keySelector">Function to select the ordering key.</param>
    /// <param name="isAscending">If <c>true</c>, orders ascending; otherwise, descending.</param>
    /// <returns>An ordered enumerable collection.</returns>
    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool isAscending)
    {
        return isAscending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
    }

    /// <summary>
    /// Adds a range of items to a HashSet.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <param name="hs">The HashSet to add items to.</param>
    /// <param name="items">The items to add. Null items are safely ignored.</param>
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

    /// <summary>
    /// Maps items through a dictionary lookup, returning the mapped values.
    /// </summary>
    /// <typeparam name="TInput">The type of input items.</typeparam>
    /// <typeparam name="K">The type of dictionary keys.</typeparam>
    /// <typeparam name="V">The type of dictionary values.</typeparam>
    /// <param name="items">The items to map.</param>
    /// <param name="map">The dictionary to use for mapping.</param>
    /// <param name="keyGetter">Function to extract the lookup key from each item.</param>
    /// <param name="omitMissing">If <c>true</c>, omits items not found in the map; otherwise, includes default values.</param>
    /// <returns>A list of mapped values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when map or keyGetter is null.</exception>
    public static IList<V> Map<TInput, K, V>(this IEnumerable<TInput> items, IDictionary<K, V> map, Func<TInput, K> keyGetter, bool omitMissing = false)
    {
        return items.Map(map, keyGetter, z => z, omitMissing);
    }

    /// <summary>
    /// Maps items through a dictionary lookup and transforms the result.
    /// </summary>
    /// <typeparam name="TInput">The type of input items.</typeparam>
    /// <typeparam name="K">The type of dictionary keys.</typeparam>
    /// <typeparam name="V">The type of dictionary values.</typeparam>
    /// <typeparam name="O">The type of output after transformation.</typeparam>
    /// <param name="items">The items to map.</param>
    /// <param name="map">The dictionary to use for mapping.</param>
    /// <param name="keyGetter">Function to extract the lookup key from each item.</param>
    /// <param name="outputTransformer">Function to transform mapped values to output type.</param>
    /// <param name="omitMissing">If <c>true</c>, omits items not found in the map; otherwise, includes default values.</param>
    /// <returns>A list of transformed mapped values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when map, keyGetter, or outputTransformer is null.</exception>
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

    /// <summary>
    /// Converts an enumerable to a dictionary, keeping the last value when duplicate keys are encountered.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
    /// <typeparam name="TSource">The type of source items.</typeparam>
    /// <param name="items">The items to convert.</param>
    /// <param name="keySelector">Function to extract the key from each item.</param>
    /// <returns>A dictionary with unique keys, keeping the last occurrence of each key.</returns>
    public static IDictionary<TKey, TSource> ToDictionaryOnConflictKeepLast<TKey, TSource>(this IEnumerable<TSource> items, Func<TSource, TKey> keySelector)
        => items.ToDictionaryOnConflictKeepLast(keySelector, z => z);

    /// <summary>
    /// Converts an enumerable to a dictionary with custom value selection, keeping the last value when duplicate keys are encountered.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
    /// <typeparam name="TVal">The type of dictionary values.</typeparam>
    /// <typeparam name="TSource">The type of source items.</typeparam>
    /// <param name="items">The items to convert.</param>
    /// <param name="keySelector">Function to extract the key from each item.</param>
    /// <param name="valSelector">Function to extract the value from each item.</param>
    /// <returns>A dictionary with unique keys, keeping the last occurrence of each key.</returns>
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

    /// <summary>
    /// Converts an enumerable to a MultipleValueDictionary where each key can have multiple values.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
    /// <typeparam name="TSource">The type of source items.</typeparam>
    /// <param name="items">The items to convert.</param>
    /// <param name="keySelector">Function to extract the key from each item.</param>
    /// <returns>A MultipleValueDictionary containing all items grouped by key.</returns>
    public static MultipleValueDictionary<TKey, TSource> ToMultipleValueDictionary<TKey, TSource>(this IEnumerable<TSource> items, Func<TSource, TKey> keySelector)
        => items.ToMultipleValueDictionary(keySelector, z => z);

    /// <summary>
    /// Converts an enumerable to a MultipleValueDictionary with custom value selection where each key can have multiple values.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
    /// <typeparam name="TVal">The type of dictionary values.</typeparam>
    /// <typeparam name="TSource">The type of source items.</typeparam>
    /// <param name="items">The items to convert.</param>
    /// <param name="keySelector">Function to extract the key from each item.</param>
    /// <param name="valSelector">Function to extract the value from each item.</param>
    /// <returns>A MultipleValueDictionary containing all items grouped by key.</returns>
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

    /// <summary>
    /// Finds the first value in a string-object dictionary that is of the specified type.
    /// </summary>
    /// <typeparam name="R">The type to search for.</typeparam>
    /// <param name="d">The dictionary to search.</param>
    /// <returns>The first value of type R found, or the default value of R if none found.</returns>
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
    /// Randomizes the order of elements in a list using the Fisher-Yates shuffle algorithm.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle in place.</param>
    /// <param name="random">The random number generator to use. If null, uses the default random generator.</param>
    /// <exception cref="ArgumentNullException">Thrown when list is null.</exception>
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

    /// <summary>
    /// Removes and returns the first element from a list (queue-style operation).
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to dequeue from.</param>
    /// <returns>The first element in the list.</returns>
    public static T Dequeue<T>(this IList<T> list)
    {
        var item = list[0];
        list.RemoveAt(0);
        return item;
    }

    /// <summary>
    /// Removes and returns the last element from a list (stack-style operation).
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to pop from.</param>
    /// <returns>The last element in the list.</returns>
    public static T Pop<T>(this IList<T> list)
    {
        var item = list[^1];
        list.RemoveAt(list.Count - 1);
        return item;
    }

    /// <summary>
    /// Returns a random element from a list, or the default value if the list is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to select from.</param>
    /// <param name="random">The random number generator to use. If null, uses the default random generator.</param>
    /// <returns>A randomly selected element, or default(T) if the list is empty.</returns>
    public static T RandomElement<T>(this IList<T> list, Random random = null)
    {
        if (list.Count == 0) return default;
        random ??= Stuff.Random;
        var i = random.Next(list.Count);
        return list[i];
    }

    /// <summary>
    /// Creates a read-only wrapper around an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The items to wrap.</param>
    /// <returns>A read-only list containing the items, or an empty list if items is null.</returns>
    public static IList<T> AsReadOnly<T>(this IEnumerable<T> items)
    {
        return items == null ? (new T[0]) : new List<T>(items).AsReadOnly();
    }

    /// <summary>
    /// Creates a read-only wrapper around a dictionary.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to wrap.</param>
    /// <returns>A read-only dictionary.</returns>
    public static IDictionary<K, V> AsReadOnlyDictionary<K, V>(this IDictionary<K, V> dict)
    {
        return new ReadonlyDictionary<K, V>(dict);
    }

    /// <summary>
    /// Filters an enumerable to exclude null elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection (must be a reference type).</typeparam>
    /// <param name="items">The enumerable to filter.</param>
    /// <returns>An enumerable containing only non-null elements.</returns>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> items) where T : class
    {
        return items.Where(i => i != null);
    }

    /// <summary>
    /// Adds a formatted string to a collection.
    /// </summary>
    /// <param name="col">The collection to add to.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    public static void AddFormat(this ICollection<string> col, string format, params object[] args)
    {
        var msg = string.Format(format, args);
        col.Add(msg);
    }

    /// <summary>
    /// Adds a formatted string to a MultipleValueDictionary.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <param name="m">The MultipleValueDictionary to add to.</param>
    /// <param name="key">The key to add the value under.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    public static void AddFormat<K>(this MultipleValueDictionary<K, string> m, K key, string format, params object[] args)
    {
        var msg = string.Format(format, args);
        m.Add(key, msg);
    }

    /// <summary>
    /// Formats an enumerable as a string with a separator and optional format template.
    /// </summary>
    /// <param name="e">The enumerable to format.</param>
    /// <param name="sep">The separator to use between elements. Defaults to empty string.</param>
    /// <param name="format">The format template for each element. {0} is the element, {1} is the index. Defaults to "{0}".</param>
    /// <returns>A formatted string representation of the enumerable.</returns>
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

    /// <summary>
    /// Formats an enumerable as a string using a custom formatter function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="e">The enumerable to format.</param>
    /// <param name="sep">The separator to use between elements.</param>
    /// <param name="formatter">Function to format each element (receives element and index).</param>
    /// <returns>A formatted string representation of the enumerable.</returns>
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

    /// <summary>
    /// Determines whether an enumerable contains any elements.
    /// </summary>
    /// <param name="e">The enumerable to check.</param>
    /// <returns><c>true</c> if the enumerable is not null and contains at least one element; otherwise, <c>false</c>.</returns>
    public static bool HasData(this IEnumerable e)
    {
        if (e == null) return false;
        var z = e.GetEnumerator();
        return z.MoveNext();
    }

    /// <summary>
    /// Gets a value from a dictionary, returning a fallback value if the key is not found.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    /// <param name="d">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="fallback">The value to return if the key is not found. Defaults to default(V).</param>
    /// <returns>The value associated with the key, or the fallback value if the key is null or not found.</returns>
    public static V GetValue<K, V>(this IDictionary<K, V> d, K key, V fallback = default)
        => key != null && d.TryGetValue(key, out var ret) ? ret : fallback;

    /// <summary>
    /// Executes an action on each element in an enumerable and returns the enumerable for chaining.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="items">The enumerable to process.</param>
    /// <param name="action">The action to execute on each element.</param>
    /// <returns>The original enumerable, allowing method chaining.</returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        if (items != null)
        {
            foreach (var d in items) { action(d); }
        }
        return items;
    }

    /// <summary>
    /// Converts an enumerable to a HashSet.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="items">The items to convert.</param>
    /// <returns>A HashSet containing the unique items.</returns>
    public static HashSet<T> ToSet<T>(this IEnumerable<T> items)
    {
        return items.ToSet(i => i);
    }

    /// <summary>
    /// Converts an enumerable to a HashSet with a transformation function.
    /// </summary>
    /// <typeparam name="TIn">The type of input elements.</typeparam>
    /// <typeparam name="TOut">The type of output elements.</typeparam>
    /// <param name="items">The items to convert.</param>
    /// <param name="converter">Function to transform each item before adding to the set.</param>
    /// <returns>A HashSet containing the unique transformed items.</returns>
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

    /// <summary>
    /// Converts an enumerable of strings to a case-insensitive HashSet.
    /// </summary>
    /// <param name="items">The strings to convert.</param>
    /// <returns>A HashSet with case-insensitive string comparison.</returns>
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

    /// <summary>
    /// Converts all elements in an enumerable using a transformation function.
    /// </summary>
    /// <typeparam name="T">The type of input elements.</typeparam>
    /// <typeparam name="TOutput">The type of output elements.</typeparam>
    /// <param name="datas">The items to convert.</param>
    /// <param name="converter">Function to transform each item.</param>
    /// <returns>A list containing the converted items.</returns>
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

    /// <summary>
    /// Removes all elements from a collection that match a predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="col">The collection to remove from.</param>
    /// <param name="toRemove">Predicate to determine which elements to remove.</param>
    /// <returns>A list of the removed items.</returns>
    public static IList<T> Remove<T>(this ICollection<T> col, Func<T, bool> toRemove)
    {
        if (col == null || toRemove == null) return Array.Empty<T>();
        List<T> removes = [];
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

    /// <summary>
    /// Removes multiple items from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="col">The collection to remove from.</param>
    /// <param name="items">The items to remove.</param>
    /// <returns>The number of items successfully removed.</returns>
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

    /// <summary>
    /// Sorts an enumerable of comparable items and returns them as a list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection (must implement IComparable).</typeparam>
    /// <param name="items">The items to sort.</param>
    /// <returns>A sorted list of the items.</returns>
    public static IList<T> OrderBy<T>(this IEnumerable<T> items) where T : IComparable
    {
        var l = new List<T>(items);
        l.Sort();
        return l;
    }

    /// <summary>
    /// Determines whether a HashSet contains any elements from another collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <param name="set">The HashSet to check.</param>
    /// <param name="other">The collection to check for overlap.</param>
    /// <returns><c>true</c> if the set contains at least one element from other; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Creates an ordered list of values from a dictionary based on a sequence of keys.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    /// <param name="d">The dictionary to get values from.</param>
    /// <param name="orderedKeys">The sequence of keys in the desired order.</param>
    /// <param name="throwOnMiss">If <c>true</c>, throws an exception when a key is not found; otherwise, uses missingVal.</param>
    /// <param name="missingVal">The value to use for missing keys. Defaults to default(V).</param>
    /// <returns>A list of values in the order specified by orderedKeys.</returns>
    /// <exception cref="InvalidMappingException">Thrown when throwOnMiss is true and a key is not found.</exception>
    public static List<V> ToOrderedValuesList<K, V>(this IDictionary<K, V> d, IEnumerable<K> orderedKeys, bool throwOnMiss = false, V missingVal = default)
    {
        var ret = new List<V>(d.Count);
        foreach (var k in orderedKeys)
        {
            if (k != null && d.TryGetValue(k, out var v))
            {
                Stuff.NoOp();
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
    /// Gets an item from an enumerable at a specific index.
    /// </summary>
    /// <param name="e">The enumerable to get the item from.</param>
    /// <param name="itemNum">The zero-based index of the item to retrieve.</param>
    /// <param name="missing">The value to return if the enumerable doesn't have enough items.</param>
    /// <returns>The item at the specified index, or the missing value if not found.</returns>
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

    /// <summary>
    /// Splits an enumerable into chunks of a specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="items">The items to chunk.</param>
    /// <param name="sublistSize">The maximum size of each chunk.</param>
    /// <returns>A list of lists, where each inner list contains up to sublistSize items.</returns>
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

    /// <summary>
    /// Increments an integer value in a dictionary by 1.
    /// If the key doesn't exist, it's initialized to 1.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <param name="d">The dictionary to update.</param>
    /// <param name="key">The key to increment.</param>
    /// <returns>The new value after incrementing.</returns>
    public static int Increment<K>(this IDictionary<K, int> d, K key)
    {
        return d.Increment(key, 1);
    }

    /// <summary>
    /// Increments an integer value in a dictionary by a specified amount.
    /// If the key doesn't exist, it's initialized to the increment amount.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <param name="d">The dictionary to update.</param>
    /// <param name="key">The key to increment.</param>
    /// <param name="incrementAmount">The amount to increment by.</param>
    /// <returns>The new value after incrementing.</returns>
    public static int Increment<K>(this IDictionary<K, int> d, K key, int incrementAmount)
    {
        return d.Increment(key, incrementAmount, incrementAmount);
    }

    /// <summary>
    /// Increments an integer value in a dictionary by a specified amount with a custom initial value.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <param name="d">The dictionary to update.</param>
    /// <param name="key">The key to increment.</param>
    /// <param name="incrementAmount">The amount to increment by if the key exists.</param>
    /// <param name="initialAmount">The value to use if the key doesn't exist.</param>
    /// <returns>The new value after incrementing.</returns>
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

    /// <summary>
    /// Finds a value in a dictionary or returns a fallback value if not found.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    /// <param name="d">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="missing">The value to return if the key is not found.</param>
    /// <returns>The value associated with the key, or the missing value if not found.</returns>
    public static V FindOrMissing<K, V>(this IDictionary<K, V> d, K key, V missing)
    {
        if (!d.TryGetValue(key, out var ret))
        {
            ret = missing;
        }
        return ret;
    }

    /// <summary>
    /// Finds a value in a dictionary or returns the default value if not found.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    /// <param name="d">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value associated with the key, or default(V) if not found.</returns>
    public static V FindOrDefault<K, V>(this IDictionary<K, V> d, K key)
    {
        if (!d.TryGetValue(key, out var ret))
        {
            ret = default;
        }
        return ret;
    }

    /// <summary>
    /// Finds a value in a dictionary or creates and stores a new value if not found.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    /// <param name="d">The dictionary to search or update.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="creator">Function to create a new value if the key is not found.</param>
    /// <returns>The existing value or the newly created value.</returns>
    public static V FindOrCreate<K, V>(this IDictionary<K, V> d, K key, Func<V> creator)
    {
        if (!d.TryGetValue(key, out var ret))
        {
            ret = creator();
            d[key] = ret;
        }
        return ret;
    }

    /// <summary>
    /// Finds a value in a dictionary or creates and stores a new value using the key if not found.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    /// <param name="d">The dictionary to search or update.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="creator">Function that uses the key to create a new value if not found.</param>
    /// <returns>The existing value or the newly created value.</returns>
    public static V FindOrCreate<K, V>(this IDictionary<K, V> d, K key, Func<K, V> creator)
    {
        if (!d.TryGetValue(key, out var ret))
        {
            ret = creator(key);
            d[key] = ret;
        }
        return ret;
    }

    /// <summary>
    /// Finds the index of the nth occurrence of an item matching a predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="items">The list to search.</param>
    /// <param name="test">Predicate to test each item.</param>
    /// <param name="nthOccurrence">Which occurrence to find (1-based). Must be non-negative.</param>
    /// <param name="zeroThValue">Value to return if nthOccurrence is 0.</param>
    /// <param name="missingValue">Value to return if the nth occurrence is not found.</param>
    /// <returns>The index of the nth occurrence, or the appropriate special value.</returns>
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

    /// <summary>
    /// Finds the index of the nth occurrence of a specific item.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="items">The list to search.</param>
    /// <param name="match">The item to find.</param>
    /// <param name="nthOccurrence">Which occurrence to find (1-based). Must be non-negative.</param>
    /// <param name="zeroThValue">Value to return if nthOccurrence is 0.</param>
    /// <param name="missingValue">Value to return if the nth occurrence is not found.</param>
    /// <returns>The index of the nth occurrence, or the appropriate special value.</returns>
    public static int? IndexOfOccurrence<T>(this IList<T> items, T match, int nthOccurrence, int? zeroThValue = null, int? missingValue = null)
       => items.IndexOfOccurrence(i =>
       {
           return i == null ? match == null : i.Equals(match);
       }, nthOccurrence, zeroThValue, missingValue);

    /// <summary>
    /// Adds a key-value pair to a list of string pairs with fluent syntax.
    /// </summary>
    /// <param name="items">The list to add to.</param>
    /// <param name="key">The key to add.</param>
    /// <param name="val">The value to add.</param>
    /// <param name="addIfNullOrWhiteSpace">If <c>false</c>, skips adding if value is null or whitespace.</param>
    /// <param name="addThis">If <c>false</c>, skips adding regardless of value.</param>
    /// <returns>The original list for method chaining.</returns>
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

    /// <summary>
    /// Adds an item to a collection and returns the collection for method chaining.
    /// </summary>
    /// <typeparam name="TColl">The type of the collection.</typeparam>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="col">The collection to add to.</param>
    /// <param name="item">The item to add.</param>
    /// <returns>The original collection for method chaining.</returns>
    public static TColl FluentAdd<TColl, T>(this TColl col, T item) where TColl : ICollection<T>
    {
        col.Add(item);
        return col;
    }

    /// <summary>
    /// Adds an item to a list and returns the list for method chaining.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="col">The list to add to.</param>
    /// <param name="item">The item to add.</param>
    /// <returns>The original list for method chaining.</returns>
    public static IList<T> FluentAdd<T>(this IList<T> col, T item)
    {
        col.Add(item);
        return col;
    }

    /// <summary>
    /// Adds multiple items to a collection and returns the collection for method chaining.
    /// </summary>
    /// <typeparam name="TColl">The type of the collection.</typeparam>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="col">The collection to add to.</param>
    /// <param name="items">The items to add. Null is safely ignored.</param>
    /// <returns>The original collection for method chaining.</returns>
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

    /// <summary>
    /// Clears a list and returns it for method chaining.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="col">The list to clear.</param>
    /// <returns>The cleared list for method chaining.</returns>
    public static IList<T> FluentClear<T>(this IList<T> col)
    {
        col.Clear();
        return col;
    }

    /// <summary>
    /// Executes an action on each element with its index in an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="col">The collection to iterate.</param>
    /// <param name="a">Action to execute, receiving the element and its zero-based index.</param>
    /// <exception cref="ArgumentNullException">Thrown when col or a is null.</exception>
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

    /// <summary>
    /// Converts an async enumerable to a list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the async enumerable.</typeparam>
    /// <param name="asyncEnumerable">The async enumerable to convert.</param>
    /// <returns>A task containing a list of all items from the async enumerable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when asyncEnumerable is null.</exception>
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

    /// <summary>
    /// Tries to get a value from a string-keyed dictionary using case-insensitive comparison.
    /// First attempts exact match, then lowercase, then full case-insensitive search.
    /// </summary>
    /// <typeparam name="T">The type of values in the dictionary.</typeparam>
    /// <param name="d">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="hit">When this method returns, contains the value if found.</param>
    /// <returns><c>true</c> if a matching key was found; otherwise, <c>false</c>.</returns>
    public static bool TryGetValueIgnoreCase<T>(this IDictionary<string, T> d, string key, out T hit)
    {
        if (d.NullSafeAny())
        {
            if (d.TryGetValue(key, out hit)) return true;
            var lkey = key.ToLower();
            if (lkey != key)
            {
                if (d.TryGetValue(lkey, out hit)) return true;
            }
            foreach (var kvp in d)
            {
                if (0 == string.Compare(kvp.Key, key, true))
                {
                    hit = kvp.Value;
                    return true;
                }
            }
        }
        else
        {
            hit = default;
        }
        return false;
    }


}
