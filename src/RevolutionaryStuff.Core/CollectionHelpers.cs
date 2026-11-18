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
    extension<K, V>(IDictionary<K, V> sources) where V : class
    {
        /// <summary>
        /// Sets a key-value pair in the dictionary only if the value is not null.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of values in the dictionary (must be a reference type).</typeparam>
        /// <param name="sources">The dictionary to update.</param>
        /// <param name="key">The key to set.</param>
        /// <param name="val">The value to set. If null, no operation is performed.</param>
        public void SetIfValNotNull(K key, V val)
        {
            if (val != null)
            {
                sources[key] = val;
            }
        }
    }

    extension<K, V>(IDictionary<K, V> sources)
    {
        /// <summary>
        /// Sets a key-value pair in the dictionary only if the key does not already exist.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of values in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to update.</param>
        /// <param name="key">The key to set.</param>
        /// <param name="val">The value to set.</param>
        public void SetIfKeyNotFound(K key, V val)
        {
            if (!sources.ContainsKey(key))
            {
                sources[key] = val;
            }
        }
        /// <summary>
        /// Creates a read-only wrapper around a dictionary.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of values in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to wrap.</param>
        /// <returns>A read-only dictionary.</returns>
        public IDictionary<K, V> AsReadOnlyDictionary()
        {
            return new ReadonlyDictionary<K, V>(sources);
        }
        /// <summary>
        /// Gets a value from a dictionary, returning a fallback value if the key is not found.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of values in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to search.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="fallback">The value to return if the key is not found. Defaults to default(V).</param>
        /// <returns>The value associated with the key, or the fallback value if the key is null or not found.</returns>
        public V GetValue(K key, V fallback = default)
            => key != null && sources.TryGetValue(key, out var ret) ? ret : fallback;
        /// <summary>
        /// Creates an ordered list of values from a dictionary based on a sequence of keys.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of values in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to get values from.</param>
        /// <param name="orderedKeys">The sequence of keys in the desired order.</param>
        /// <param name="throwOnMiss">If <c>true</c>, throws an exception when a key is not found; otherwise, uses missingVal.</param>
        /// <param name="missingVal">The value to use for missing keys. Defaults to default(V).</param>
        /// <returns>A list of values in the order specified by orderedKeys.</returns>
        /// <exception cref="InvalidMappingException">Thrown when throwOnMiss is true and a key is not found.</exception>
        public List<V> ToOrderedValuesList(IEnumerable<K> orderedKeys, bool throwOnMiss = false, V missingVal = default)
        {
            var ret = new List<V>(sources.Count);
            foreach (var k in orderedKeys)
            {
                if (k != null && sources.TryGetValue(k, out var v))
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
        /// Finds a value in a dictionary or returns a fallback value if not found.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of values in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to search.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="missing">The value to return if the key is not found.</param>
        /// <returns>The value associated with the key, or the missing value if not found.</returns>
        public V FindOrMissing(K key, V missing)
        {
            if (!sources.TryGetValue(key, out var ret))
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
        /// <param name="sources">The dictionary to search.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The value associated with the key, or default(V) if not found.</returns>
        public V FindOrDefault(K key)
        {
            if (!sources.TryGetValue(key, out var ret))
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
        /// <param name="sources">The dictionary to search or update.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="creator">Function to create a new value if the key is not found.</param>
        /// <returns>The existing value or the newly created value.</returns>
        public V FindOrCreate(K key, Func<V> creator)
        {
            if (!sources.TryGetValue(key, out var ret))
            {
                ret = creator();
                sources[key] = ret;
            }
            return ret;
        }
        /// <summary>
        /// Finds a value in a dictionary or creates and stores a new value using the key if not found.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of values in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to search or update.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="creator">Function that uses the key to create a new value if not found.</param>
        /// <returns>The existing value or the newly created value.</returns>
        public V FindOrCreate(K key, Func<K, V> creator)
        {
            if (!sources.TryGetValue(key, out var ret))
            {
                ret = creator(key);
                sources[key] = ret;
            }
            return ret;
        }
    }

    extension<T>(IEnumerable<T> sources)
    {
        /// <summary>
        /// Returns a null-safe enumerable, providing an empty collection if the input is null.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="sources">The enumerable to check.</param>
        /// <returns>The original enumerable if not null; otherwise, an empty array.</returns>
        public IEnumerable<T> NullSafeEnumerable()
        {
            return sources ?? (new T[0]);
        }
        /// <summary>
        /// Returns the count of elements in an enumerable, handling null safely by returning 0.
        /// Optimized to use IList.Count when available.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="sources">The enumerable to count.</param>
        /// <returns>The number of elements, or 0 if the enumerable is null.</returns>
        public int NullSafeCount()
        {
            return sources == null ? 0 : sources is IList l ? l.Count : sources.Count();
        }
        /// <summary>
        /// Determines whether an enumerable contains any elements, with optional predicate filtering.
        /// Returns false for null enumerables.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="sources">The enumerable to check.</param>
        /// <param name="predicate">Optional predicate to filter elements.</param>
        /// <returns><c>true</c> if the enumerable is not null and contains matching elements; otherwise, <c>false</c>.</returns>
        public bool NullSafeAny(Func<T, bool> predicate = null)
            => sources != null && (predicate == null ? sources.Any() : sources.Any(predicate));
        /// <summary>
        /// Creates a read-only wrapper around an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="sources">The items to wrap.</param>
        /// <returns>A read-only list containing the items, or an empty list if items is null.</returns>
        public IList<T> AsReadOnly()
        {
            return sources == null ? (new T[0]) : new List<T>(sources).AsReadOnly();
        }
        /// <summary>
        /// Formats an enumerable as a string using a custom formatter function.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="sources">The enumerable to format.</param>
        /// <param name="sep">The separator to use between elements.</param>
        /// <param name="formatter">Function to format each element (receives element and index).</param>
        /// <returns>A formatted string representation of the enumerable.</returns>
        public string Format(string sep, Func<T, int, string> formatter)
        {
            if (null == sources) return "";
            var sb = new StringBuilder();
            var x = 0;
            foreach (var o in sources)
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
        /// Executes an action on each element in an enumerable and returns the enumerable for chaining.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="sources">The enumerable to process.</param>
        /// <param name="action">The action to execute on each element.</param>
        /// <returns>The original enumerable, allowing method chaining.</returns>
        public IEnumerable<T> ForEach(Action<T> action)
        {
            if (sources != null)
            {
                foreach (var d in sources) { action(d); }
            }
            return sources;
        }
        /// <summary>
        /// Converts an enumerable to a HashSet.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="sources">The items to convert.</param>
        /// <returns>A HashSet containing the unique items.</returns>
        public HashSet<T> ToSet()
        {
            return sources.ToSet(i => i);
        }
        /// <summary>
        /// Converts all elements in an enumerable using a transformation function.
        /// </summary>
        /// <typeparam name="T">The type of input elements.</typeparam>
        /// <typeparam name="TOutput">The type of output elements.</typeparam>
        /// <param name="sources">The items to convert.</param>
        /// <param name="converter">Function to transform each item.</param>
        /// <returns>A list containing the converted items.</returns>
        public List<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter)
        {
            var ret = new List<TOutput>();
            if (sources != null)
            {
                foreach (var data in sources)
                {
                    ret.Add(converter(data));
                }
            }
            return ret;
        }
        /// <summary>
        /// Splits an enumerable into chunks of a specified size.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="sources">The items to chunk.</param>
        /// <param name="sublistSize">The maximum size of each chunk.</param>
        /// <returns>A list of lists, where each inner list contains up to sublistSize items.</returns>
        public IList<IList<T>> Chunkify(int sublistSize)
        {
            var ret = new List<IList<T>>();
            List<T> sub = null;
            foreach (var i in sources)
            {
                if (sub == null)
                {
                    sub = [with(sublistSize)];
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
        /// Executes an action on each element with its index in an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="sources">The collection to iterate.</param>
        /// <param name="a">Action to execute, receiving the element and its zero-based index.</param>
        /// <exception cref="ArgumentNullException">Thrown when col or a is null.</exception>
        public void ForEach(Action<T, int> a)
        {
            ArgumentNullException.ThrowIfNull(sources);
            ArgumentNullException.ThrowIfNull(a);

            var z = 0;
            foreach (var item in sources)
            {
                a(item, z++);
            }
        }
    }





    extension(IEnumerable<KeyValuePair<string, object>> sources)
    {
        /// <summary>
        /// Converts a collection of string-object key-value pairs to string-string key-value pairs.
        /// </summary>
        /// <param name="sources">The collection of key-value pairs to convert.</param>
        /// <returns>A list of string-string key-value pairs.</returns>
        public IList<KeyValuePair<string, string>> ToStringStringKeyValuePairs()
        {
            var ret = new List<KeyValuePair<string, string>>();
            if (sources != null)
            {
                foreach (var kvp in sources)
                {
                    ret.Add(new KeyValuePair<string, string>(kvp.Key, Stuff.ObjectToString(kvp.Value)));
                }
            }
            return ret;
        }
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

    extension<TSource>(IQueryable<TSource> sources)
    {
        /// <summary>
        /// Orders a queryable collection by a key selector in ascending or descending order.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
        /// <typeparam name="TKey">The type of the ordering key.</typeparam>
        /// <param name="sources">The queryable collection to order.</param>
        /// <param name="keySelector">Expression to select the ordering key.</param>
        /// <param name="isAscending">If <c>true</c>, orders ascending; otherwise, descending.</param>
        /// <returns>An ordered queryable collection.</returns>
        public IOrderedQueryable<TSource> OrderBy<TKey>(Expression<Func<TSource, TKey>> keySelector, bool isAscending)
        {
            return isAscending ? sources.OrderBy(keySelector) : sources.OrderByDescending(keySelector);
        }
    }

    extension<TSource>(IEnumerable<TSource> sources)
    {
        /// <summary>
        /// Orders an enumerable collection by a key selector in ascending or descending order.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
        /// <typeparam name="TKey">The type of the ordering key.</typeparam>
        /// <param name="sources">The enumerable collection to order.</param>
        /// <param name="keySelector">Function to select the ordering key.</param>
        /// <param name="isAscending">If <c>true</c>, orders ascending; otherwise, descending.</param>
        /// <returns>An ordered enumerable collection.</returns>
        public IOrderedEnumerable<TSource> OrderBy<TKey>(Func<TSource, TKey> keySelector, bool isAscending)
        {
            return isAscending ? sources.OrderBy(keySelector) : sources.OrderByDescending(keySelector);
        }
        /// <summary>
        /// Converts an enumerable to a dictionary, keeping the last value when duplicate keys are encountered.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TSource">The type of source items.</typeparam>
        /// <param name="sources">The items to convert.</param>
        /// <param name="keySelector">Function to extract the key from each item.</param>
        /// <returns>A dictionary with unique keys, keeping the last occurrence of each key.</returns>
        public IDictionary<TKey, TSource> ToDictionaryOnConflictKeepLast<TKey>(Func<TSource, TKey> keySelector)
            => sources.ToDictionaryOnConflictKeepLast(keySelector, z => z);
        /// <summary>
        /// Converts an enumerable to a dictionary with custom value selection, keeping the last value when duplicate keys are encountered.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TVal">The type of dictionary values.</typeparam>
        /// <typeparam name="TSource">The type of source items.</typeparam>
        /// <param name="sources">The items to convert.</param>
        /// <param name="keySelector">Function to extract the key from each item.</param>
        /// <param name="valSelector">Function to extract the value from each item.</param>
        /// <returns>A dictionary with unique keys, keeping the last occurrence of each key.</returns>
        public IDictionary<TKey, TVal> ToDictionaryOnConflictKeepLast<TKey, TVal>(Func<TSource, TKey> keySelector, Func<TSource, TVal> valSelector)
        {
            var d = new Dictionary<TKey, TVal>();
            if (sources != null)
            {
                foreach (var item in sources)
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
        /// <param name="sources">The items to convert.</param>
        /// <param name="keySelector">Function to extract the key from each item.</param>
        /// <returns>A MultipleValueDictionary containing all items grouped by key.</returns>
        public MultipleValueDictionary<TKey, TSource> ToMultipleValueDictionary<TKey>(Func<TSource, TKey> keySelector)
            => sources.ToMultipleValueDictionary(keySelector, z => z);
        /// <summary>
        /// Converts an enumerable to a MultipleValueDictionary with custom value selection where each key can have multiple values.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TVal">The type of dictionary values.</typeparam>
        /// <typeparam name="TSource">The type of source items.</typeparam>
        /// <param name="sources">The items to convert.</param>
        /// <param name="keySelector">Function to extract the key from each item.</param>
        /// <param name="valSelector">Function to extract the value from each item.</param>
        /// <returns>A MultipleValueDictionary containing all items grouped by key.</returns>
        public MultipleValueDictionary<TKey, TVal> ToMultipleValueDictionary<TKey, TVal>(Func<TSource, TKey> keySelector, Func<TSource, TVal> valSelector)
        {
            var m = new MultipleValueDictionary<TKey, TVal>();
            if (sources != null)
            {
                foreach (var item in sources)
                {
                    m.Add(keySelector(item), valSelector(item));
                }
            }
            return m;
        }
    }

    extension<T>(HashSet<T> sources)
    {
        /// <summary>
        /// Adds a range of items to a HashSet.
        /// </summary>
        /// <typeparam name="T">The type of elements in the set.</typeparam>
        /// <param name="sources">The HashSet to add items to.</param>
        /// <param name="items">The items to add. Null items are safely ignored.</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    sources.Add(item);
                }
            }
        }
        /// <summary>
        /// Determines whether a HashSet contains any elements from another collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the set.</typeparam>
        /// <param name="sources">The HashSet to check.</param>
        /// <param name="other">The collection to check for overlap.</param>
        /// <returns><c>true</c> if the set contains at least one element from other; otherwise, <c>false</c>.</returns>
        public bool ContainsAnyElement(IEnumerable<T> other)
        {
            if (other != null)
            {
                foreach (var z in other)
                {
                    if (sources.Contains(z)) return true;
                }
            }
            return false;
        }
    }

    extension<TInput>(IEnumerable<TInput> sources)
    {
        /// <summary>
        /// Maps items through a dictionary lookup, returning the mapped values.
        /// </summary>
        /// <typeparam name="TInput">The type of input items.</typeparam>
        /// <typeparam name="K">The type of dictionary keys.</typeparam>
        /// <typeparam name="V">The type of dictionary values.</typeparam>
        /// <param name="sources">The items to map.</param>
        /// <param name="map">The dictionary to use for mapping.</param>
        /// <param name="keyGetter">Function to extract the lookup key from each item.</param>
        /// <param name="omitMissing">If <c>true</c>, omits items not found in the map; otherwise, includes default values.</param>
        /// <returns>A list of mapped values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when map or keyGetter is null.</exception>
        public IList<V> Map<K, V>(IDictionary<K, V> map, Func<TInput, K> keyGetter, bool omitMissing = false)
        {
            return sources.Map(map, keyGetter, z => z, omitMissing);
        }
        /// <summary>
        /// Maps items through a dictionary lookup and transforms the result.
        /// </summary>
        /// <typeparam name="TInput">The type of input items.</typeparam>
        /// <typeparam name="K">The type of dictionary keys.</typeparam>
        /// <typeparam name="V">The type of dictionary values.</typeparam>
        /// <typeparam name="O">The type of output after transformation.</typeparam>
        /// <param name="sources">The items to map.</param>
        /// <param name="map">The dictionary to use for mapping.</param>
        /// <param name="keyGetter">Function to extract the lookup key from each item.</param>
        /// <param name="outputTransformer">Function to transform mapped values to output type.</param>
        /// <param name="omitMissing">If <c>true</c>, omits items not found in the map; otherwise, includes default values.</param>
        /// <returns>A list of transformed mapped values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when map, keyGetter, or outputTransformer is null.</exception>
        public IList<O> Map<K, V, O>(IDictionary<K, V> map, Func<TInput, K> keyGetter, Func<V, O> outputTransformer, bool omitMissing = false)
        {
            ArgumentNullException.ThrowIfNull(map);
            ArgumentNullException.ThrowIfNull(keyGetter);
            ArgumentNullException.ThrowIfNull(outputTransformer);

            var ret = new List<O>();
            foreach (var item in sources)
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
    }











    extension(IDictionary<string, object> sources)
    {
        /// <summary>
        /// Finds the first value in a string-object dictionary that is of the specified type.
        /// </summary>
        /// <typeparam name="R">The type to search for.</typeparam>
        /// <param name="sources">The dictionary to search.</param>
        /// <returns>The first value of type R found, or the default value of R if none found.</returns>
        public R FirstValueOfType<R>()
        {
            if (sources != null)
            {
                foreach (var v in sources.Values)
                {
                    if (v is R) return (R)v;
                }
            }
            return default;
        }
    }

    extension<T>(IList<T> sources)
    {
        /// <summary>
        /// Randomizes the order of elements in a list using the Fisher-Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="sources">The list to shuffle in place.</param>
        /// <param name="random">The random number generator to use. If null, uses the default random generator.</param>
        /// <exception cref="ArgumentNullException">Thrown when list is null.</exception>
        public void Shuffle(Random random = null)
        {
            ArgumentNullException.ThrowIfNull(sources);
            var len = sources.Count;
            if (len < 2) return;
            random ??= Stuff.Random;
            int x, y;
            T o;
            for (x = 0; x < len; ++x)
            {
                y = random.Next(len);
                o = sources[x];
                sources[x] = sources[y];
                sources[y] = o;
            }
        }
        /// <summary>
        /// Removes and returns the first element from a list (queue-style operation).
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="sources">The list to dequeue from.</param>
        /// <returns>The first element in the list.</returns>
        public T Dequeue()
        {
            var item = sources[0];
            sources.RemoveAt(0);
            return item;
        }
        /// <summary>
        /// Removes and returns the last element from a list (stack-style operation).
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="sources">The list to pop from.</param>
        /// <returns>The last element in the list.</returns>
        public T Pop()
        {
            var item = sources[^1];
            sources.RemoveAt(sources.Count - 1);
            return item;
        }
        /// <summary>
        /// Returns a random element from a list, or the default value if the list is empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="sources">The list to select from.</param>
        /// <param name="random">The random number generator to use. If null, uses the default random generator.</param>
        /// <returns>A randomly selected element, or default(T) if the list is empty.</returns>
        public T RandomElement(Random random = null)
        {
            if (sources.Count == 0) return default;
            random ??= Stuff.Random;
            var i = random.Next(sources.Count);
            return sources[i];
        }
        /// <summary>
        /// Finds the index of the nth occurrence of an item matching a predicate.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="sources">The list to search.</param>
        /// <param name="test">Predicate to test each item.</param>
        /// <param name="nthOccurrence">Which occurrence to find (1-based). Must be non-negative.</param>
        /// <param name="zeroThValue">Value to return if nthOccurrence is 0.</param>
        /// <param name="missingValue">Value to return if the nth occurrence is not found.</param>
        /// <returns>The index of the nth occurrence, or the appropriate special value.</returns>
        public int? IndexOfOccurrence(Func<T, bool> test, int nthOccurrence, int? zeroThValue = null, int? missingValue = null)
        {
            Requires.NonNegative(nthOccurrence);

            if (nthOccurrence == 0) return zeroThValue;

            var cnt = 0;
            for (var z = 0; z < sources.Count; ++z)
            {
                var i = sources[z];
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
        /// <param name="sources">The list to search.</param>
        /// <param name="match">The item to find.</param>
        /// <param name="nthOccurrence">Which occurrence to find (1-based). Must be non-negative.</param>
        /// <param name="zeroThValue">Value to return if nthOccurrence is 0.</param>
        /// <param name="missingValue">Value to return if the nth occurrence is not found.</param>
        /// <returns>The index of the nth occurrence, or the appropriate special value.</returns>
        public int? IndexOfOccurrence(T match, int nthOccurrence, int? zeroThValue = null, int? missingValue = null)
           => sources.IndexOfOccurrence(i =>
           {
               return i == null ? match == null : i.Equals(match);
           }, nthOccurrence, zeroThValue, missingValue);
        /// <summary>
        /// Adds an item to a list and returns the list for method chaining.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="sources">The list to add to.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>The original list for method chaining.</returns>
        public IList<T> FluentAdd(T item)
        {
            sources.Add(item);
            return sources;
        }
        /// <summary>
        /// Clears a list and returns it for method chaining.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="sources">The list to clear.</param>
        /// <returns>The cleared list for method chaining.</returns>
        public IList<T> FluentClear()
        {
            sources.Clear();
            return sources;
        }
    }











    extension<T>(IEnumerable<T> sources) where T : class
    {
        /// <summary>
        /// Filters an enumerable to exclude null elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection (must be a reference type).</typeparam>
        /// <param name="sources">The enumerable to filter.</param>
        /// <returns>An enumerable containing only non-null elements.</returns>
        public IEnumerable<T> WhereNotNull()
        {
            return sources.Where(i => i != null);
        }
    }

    extension(ICollection<string> sources)
    {
        /// <summary>
        /// Adds a formatted string to a collection.
        /// </summary>
        /// <param name="sources">The collection to add to.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        public void AddFormat(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            sources.Add(msg);
        }
    }

    extension<K>(MultipleValueDictionary<K, string> sources)
    {
        /// <summary>
        /// Adds a formatted string to a MultipleValueDictionary.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <param name="sources">The MultipleValueDictionary to add to.</param>
        /// <param name="key">The key to add the value under.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        public void AddFormat(K key, string format, params object[] args)
        {
            var msg = string.Format(format, args);
            sources.Add(key, msg);
        }
    }

    extension(IEnumerable sources)
    {
        /// <summary>
        /// Formats an enumerable as a string with a separator and optional format template.
        /// </summary>
        /// <param name="sources">The enumerable to format.</param>
        /// <param name="sep">The separator to use between elements. Defaults to empty string.</param>
        /// <param name="format">The format template for each element. {0} is the element, {1} is the index. Defaults to "{0}".</param>
        /// <returns>A formatted string representation of the enumerable.</returns>
        public string Format(string sep = "", string format = "{0}")
        {
            if (null == sources) return "";
            var sb = new StringBuilder();
            var x = 0;
            foreach (var o in sources)
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
        /// Determines whether an enumerable contains any elements.
        /// </summary>
        /// <param name="sources">The enumerable to check.</param>
        /// <returns><c>true</c> if the enumerable is not null and contains at least one element; otherwise, <c>false</c>.</returns>
        public bool HasData()
        {
            if (sources == null) return false;
            var z = sources.GetEnumerator();
            return z.MoveNext();
        }
    }











    extension<TIn>(IEnumerable<TIn> sources)
    {
        /// <summary>
        /// Converts an enumerable to a HashSet with a transformation function.
        /// </summary>
        /// <typeparam name="TIn">The type of input elements.</typeparam>
        /// <typeparam name="TOut">The type of output elements.</typeparam>
        /// <param name="sources">The items to convert.</param>
        /// <param name="converter">Function to transform each item before adding to the set.</param>
        /// <returns>A HashSet containing the unique transformed items.</returns>
        public HashSet<TOut> ToSet<TOut>(Func<TIn, TOut> converter)
        {
            var set = new HashSet<TOut>();
            if (sources != null)
            {
                foreach (var i in sources)
                {
                    set.Add(converter(i));
                }
            }
            return set;
        }
    }

    extension(IEnumerable<string> sources)
    {
        /// <summary>
        /// Converts an enumerable of strings to a case-insensitive HashSet.
        /// </summary>
        /// <param name="sources">The strings to convert.</param>
        /// <returns>A HashSet with case-insensitive string comparison.</returns>
        public HashSet<string> ToCaseInsensitiveSet()
        {
            var set = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            if (sources != null)
            {
                foreach (var s in sources)
                {
                    set.Add(s);
                }
            }
            return set;
        }
    }



    extension<T>(ICollection<T> sources)
    {
        /// <summary>
        /// Removes all elements from a collection that match a predicate.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="sources">The collection to remove from.</param>
        /// <param name="toRemove">Predicate to determine which elements to remove.</param>
        /// <returns>A list of the removed items.</returns>
        public IList<T> Remove(Func<T, bool> toRemove)
        {
            if (sources == null || toRemove == null) return Array.Empty<T>();
            List<T> removes = [];
            foreach (var item in sources)
            {
                if (toRemove(item))
                {
                    removes.Add(item);
                }
            }
            if (removes.Count > 0)
            {
                if (removes.Count == sources.Count)
                {
                    sources.Clear();
                }
                else
                {
                    sources.Remove(removes);
                }
            }
            return removes;
        }
        /// <summary>
        /// Removes multiple items from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="sources">The collection to remove from.</param>
        /// <param name="items">The items to remove.</param>
        /// <returns>The number of items successfully removed.</returns>
        public int Remove(IEnumerable<T> items)
        {
            if (sources == null || items == null) return 0;
            var cnt = 0;
            foreach (var item in items)
            {
                cnt += sources.Remove(item) ? 1 : 0;
            }
            return cnt;
        }
    }



    extension<T>(IEnumerable<T> sources) where T : IComparable
    {
        /// <summary>
        /// Sorts an enumerable of comparable items and returns them as a list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection (must implement IComparable).</typeparam>
        /// <param name="sources">The items to sort.</param>
        /// <returns>A sorted list of the items.</returns>
        public IList<T> OrderBy()
        {
            var l = new List<T>(sources);
            l.Sort();
            return l;
        }
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



    extension<K>(IDictionary<K, int> sources)
    {
        /// <summary>
        /// Increments an integer value in a dictionary by 1.
        /// If the key doesn't exist, it's initialized to 1.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to update.</param>
        /// <param name="key">The key to increment.</param>
        /// <returns>The new value after incrementing.</returns>
        public int Increment(K key)
        {
            return sources.Increment(key, 1);
        }
        /// <summary>
        /// Increments an integer value in a dictionary by a specified amount.
        /// If the key doesn't exist, it's initialized to the increment amount.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to update.</param>
        /// <param name="key">The key to increment.</param>
        /// <param name="incrementAmount">The amount to increment by.</param>
        /// <returns>The new value after incrementing.</returns>
        public int Increment(K key, int incrementAmount)
        {
            return sources.Increment(key, incrementAmount, incrementAmount);
        }
        /// <summary>
        /// Increments an integer value in a dictionary by a specified amount with a custom initial value.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to update.</param>
        /// <param name="key">The key to increment.</param>
        /// <param name="incrementAmount">The amount to increment by if the key exists.</param>
        /// <param name="initialAmount">The value to use if the key doesn't exist.</param>
        /// <returns>The new value after incrementing.</returns>
        public int Increment(K key, int incrementAmount, int initialAmount)
        {
            if (sources.TryGetValue(key, out var val))
            {
                val += incrementAmount;
            }
            else
            {
                val = initialAmount;
            }
            sources[key] = val;
            return val;
        }
    }

















    extension(IList<KeyValuePair<string, string>> sources)
    {
        /// <summary>
        /// Adds a key-value pair to a list of string pairs with fluent syntax.
        /// </summary>
        /// <param name="sources">The list to add to.</param>
        /// <param name="key">The key to add.</param>
        /// <param name="val">The value to add.</param>
        /// <param name="addIfNullOrWhiteSpace">If <c>false</c>, skips adding if value is null or whitespace.</param>
        /// <param name="addThis">If <c>false</c>, skips adding regardless of value.</param>
        /// <returns>The original list for method chaining.</returns>
        public IList<KeyValuePair<string, string>> FluentAdd(string key, string val, bool addIfNullOrWhiteSpace = true, bool addThis = true)
        {
            if (addThis)
            {
                if (addIfNullOrWhiteSpace || !string.IsNullOrWhiteSpace(val))
                {
                    sources.Add(new(key, val));
                }
            }
            return sources;
        }
    }

    extension<TColl, T>(TColl sources) where TColl : ICollection<T>
    {
        /// <summary>
        /// Adds an item to a collection and returns the collection for method chaining.
        /// </summary>
        /// <typeparam name="TColl">The type of the collection.</typeparam>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="sources">The collection to add to.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>The original collection for method chaining.</returns>
        public TColl FluentAdd(T item)
        {
            sources.Add(item);
            return sources;
        }
        /// <summary>
        /// Adds multiple items to a collection and returns the collection for method chaining.
        /// </summary>
        /// <typeparam name="TColl">The type of the collection.</typeparam>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="sources">The collection to add to.</param>
        /// <param name="items">The items to add. Null is safely ignored.</param>
        /// <returns>The original collection for method chaining.</returns>
        public TColl FluentAddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var i in items)
                {
                    sources.Add(i);
                }
            }
            return sources;
        }
    }









    extension<T>(IAsyncEnumerable<T> sources)
    {
        /// <summary>
        /// Converts an async enumerable to a list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the async enumerable.</typeparam>
        /// <param name="sources">The async enumerable to convert.</param>
        /// <returns>A task containing a list of all items from the async enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when asyncEnumerable is null.</exception>
        public async Task<IList<T>> ToListAsync()
        {
            ArgumentNullException.ThrowIfNull(sources);

            if (sources == null) return null;
            var items = new List<T>();
            await foreach (var item in sources)
            {
                items.Add(item);
            }
            return items;
        }
    }

    extension<T>(IDictionary<string, T> sources)
    {
        /// <summary>
        /// Tries to get a value from a string-keyed dictionary using case-insensitive comparison.
        /// First attempts exact match, then lowercase, then full case-insensitive search.
        /// </summary>
        /// <typeparam name="T">The type of values in the dictionary.</typeparam>
        /// <param name="sources">The dictionary to search.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="hit">When this method returns, contains the value if found.</param>
        /// <returns><c>true</c> if a matching key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValueIgnoreCase(string key, out T hit)
        {
            if (sources.NullSafeAny())
            {
                if (sources.TryGetValue(key, out hit)) return true;
                var lkey = key.ToLower();
                if (lkey != key)
                {
                    if (sources.TryGetValue(lkey, out hit)) return true;
                }
                foreach (var kvp in sources)
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


}
