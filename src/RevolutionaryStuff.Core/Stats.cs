using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RevolutionaryStuff.Core
{
    /// <summary>
    /// A simple class for keeping stats / running totals
    /// </summary>
    internal class Stats : IEnumerable
    {
        /// <summary>
        /// An instance of a bitbucket
        /// </summary>
        public static readonly Stats BitBucket;

        /// <summary>
        /// Stat objects by their key
        /// </summary>
        protected readonly IDictionary<object, Stat> StatsByKey;

        /// <summary>
        /// When true, all attempts to set a value are ignored
        /// </summary>
        protected bool IsBitbucket;

        /// <summary>
        /// The name of this stat object
        /// </summary>
        public string Name;

        /// <summary>
        /// A Parent stats class used for chaining (i.e. when you update a stat in this class, it will update the parent as well)
        /// </summary>
        public Stats Parent;

        #region Constructors

        static Stats()
        {
            BitBucket = new Stats();
            BitBucket.IsBitbucket = true;
        }

        /// <summary>
        /// Construct an empty instnace of a stats class
        /// </summary>
        /// <param name="parent">The parent</param>
        /// <param name="name">The name of the stats object</param>
        /// <param name="statsByKey">The backing store [optional]</param>
        public Stats(Stats parent = null, string name = null, IDictionary<object, Stat> StatsByKey = null)
        {
            Parent = parent;
            Name = name;
            this.StatsByKey = null == StatsByKey ? new Dictionary<object, Stat>() : StatsByKey;
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        /// Grab an enumerator so we can iterate through all the contained stats
        /// </summary>
        /// <returns>An enumberator</returns>
        public IEnumerator GetEnumerator()
        {
            return StatsByKey.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// A collection of the keys to all the stats in this collection
        /// </summary>
        public ICollection<object> Keys
        {
            get { return StatsByKey.Keys; }
        }

        /// <summary>
        /// All of the stats in this collection
        /// </summary>
        public ICollection<Stat> Values
        {
            get { return StatsByKey.Values; }
        }

        /// <summary>
        /// The number of stat objects in this collection
        /// </summary>
        public int Count
        {
            get { return StatsByKey.Count; }
        }

        /// <summary>
        /// Removes all elements from the current stats set.
        /// </summary>
        public void Clear()
        {
            StatsByKey.Clear();
        }

        /// <summary>
        /// Gets the value for a particular key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value.  0 if the key does not exist</returns>
        public long GetVal(object key)
        {
            return GetVal(key, 0);
        }

        /// <summary>
        /// Gets the value for a particular key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="missing">The value to use if the key is not present</param>
        /// <returns>The value if it exists, else missing</returns>
        public long GetVal(object key, long missing)
        {
            var s = (Stat)StatsByKey[key];
            if (null == s)
            {
                return missing;
            }
            else
            {
                return s.Val;
            }
        }

        /// <summary>
        /// Gets the stat with the given key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The corresponding stat.  If no such beast exists, a new stat with of the given key</returns>
        public Stat Get(object key)
        {
            var s = (Stat)StatsByKey[key];
            if (null == s)
            {
                return new Stat(key);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Finds the Stat with the highest value
        /// </summary>
        /// <returns>Null if there are no stats, else the Stat with the highest value.  A tie is chosen arbitrarily</returns>
        public Stat FindMax()
        {
            Stat ret = null;
            foreach (Stat s in Values)
            {
                if (null == ret || s.Val > ret.Val)
                {
                    ret = s;
                }
            }
            return ret;
        }

        /// <summary>
        /// Sets the value at the key to be the max(current stat value, input)
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="val">The max</param>
        /// <returns>The value of the stat after this call</returns>
        public long Max(object key, long val)
        {
            Stat s;
            if (StatsByKey.TryGetValue(key, out s))
            {
                s.Set(Math.Max(s.Val, val));
            }
            else
            {
                s = new Stat(key, val);
                StatsByKey[key] = s;
            }
            if (null != Parent)
            {
                Parent.Max(key, val);
            }
            return s.Val;
        }

        /// <summary>
        /// Increment the value of the stat by 1
        /// If the stat does not exist, create it with a value of 1
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The resulting value</returns>
        public long Increment(object key)
        {
            return Increment(key, 1, Environment.TickCount);
        }

        /// <summary>
        /// Increment the value of the stat by Val.
        /// If the stat does not exist, create it with a value of Val
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="val">The amount to increment</param>
        /// <returns>The resulting value</returns>
        public long Increment(object key, long val)
        {
            return Increment(key, val, Environment.TickCount);
        }

        /// <summary>
        /// Increment the value of the stat by Val.
        /// If the stat does not exist, create it with a value of Val
        /// </summary>
        /// <remarks>
        /// This does nothing in release mode
        /// </remarks>
        /// <param name="key">The key</param>
        /// <param name="val">The amount to increment</param>
        [Conditional("DEBUG")]
        public void IncrementDbg(object key, long val)
        {
            Increment(key, val, Environment.TickCount);
        }

        /// <summary>
        /// Increment the value of the stat by Val.
        /// If the stat does not exist, create it with a value of Val
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="val">The amount to increment</param>
        /// <param name="tickCount">The time at which this update occurred</param>
        /// <returns>The resulting value</returns>
        public long Increment(object key, long val, int tickCount)
        {
            Requires.NonNull(key, nameof(key));
            if (IsBitbucket) return 0;
            Stat s;
            if (StatsByKey.TryGetValue(key, out s))
            {
                s.Set(s.Val + val, tickCount);
            }
            else
            {
                s = new Stat(key, val);
                StatsByKey[key] = s;
            }
            if (null != Parent)
            {
                Parent.Increment(key, val, tickCount);
            }
            return s.Val;
        }

        public void Merge(Stats stats)
        {
            Merge(stats, "{0}");
        }

        public void Merge(Stats stats, string formatName)
        {
            Requires.NonNull(stats, nameof(stats));
            if (IsBitbucket) return;
            foreach (Stat s in stats.StatsByKey.Values)
            {
                var n = new Stat(s);
                string name = String.Format(formatName, s.Key);
                n.Key = name;
                StatsByKey[name] = n;
            }
        }

        /// <summary>
        /// Set the value of the current stat to the input
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="val">The new value</param>
        /// <param name="setParent">Whether or not to set the parent's value</param>
        public void Set(object key, long val, bool setParent = true)
        {
            Requires.NonNull(key, nameof(key));
            if (IsBitbucket) return;
            Stat s;
            if (StatsByKey.TryGetValue(key, out s))
            {
                s.Set(val);
            }
            else
            {
                StatsByKey[key] = new Stat(key, val);
            }
            if (Parent != null && setParent)
            {
                Parent.Set(key, val);
            }
        }

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        /// <returns>A string with identifying information about this class</returns>
        public override string ToString()
        {
            var vals = new List<string>();
            foreach (object o in StatsByKey.Values)
            {
                vals.Add(o.ToString());
            }
            vals.Sort();
            return string.Format(
                @"<Stats name=""{0}"">
{1}
</Stats>",
                Name,
                vals.Format("\r\n", "\t{0}"));
        }

        #region Nested type: Stat

        /// <summary>
        /// An individual unit of data we are tracking
        /// </summary>
        public class Stat
        {
            /// <summary>
            /// The key
            /// </summary>
            public object Key;

            /// <summary>
            /// The last time (in ticks since windows started) that the value was modified
            /// </summary>
            public int tcLastModified;

            /// <summary>
            /// The measured data
            /// </summary>
            public long Val;

            /// <summary>
            /// The last time the value was modified
            /// </summary>
            public DateTime LastModified
            {
                get { return Stuff.TickCount2DateTime(tcLastModified); }
            }

            /// <summary>
            /// The Key's.ToString()
            /// </summary>
            public string Name
            {
                get { return Key.ToString(); }
            }

            #region Constructors

            /// <summary>
            /// Construct a new stat instance from an existing one
            /// </summary>
            /// <param name="s">A stat used for modeling</param>
            public Stat(Stat s)
            {
                Val = s.Val;
                tcLastModified = s.tcLastModified;
                Key = s.Key;
            }

            /// <summary>
            /// Construct an empty stat with the given key
            /// </summary>
            /// <param name="key">The key</param>
            public Stat(object Key)
            {
                this.Key = Key;
            }

            /// <summary>
            /// Construct an empty stat with the given key
            /// </summary>
            /// <param name="key">The key</param>
            /// <param name="val">The initial value</param>
            public Stat(object Key, long Val)
                : this(Key)
            {
                Set(Val, Environment.TickCount);
            }

            #endregion

            /// <summary>
            /// Sets the value
            /// </summary>
            /// <param name="val">The new value</param>
            public void Set(long val)
            {
                Set(val, Environment.TickCount);
            }

            /// <summary>
            /// Sets the value
            /// </summary>
            /// <param name="val">The new value</param>
            /// <param name="tickCount">The ticks (since windows started) at which point this change took place</param>
            public void Set(long val, int tickCount)
            {
                Interlocked.Exchange(ref tcLastModified, tickCount);
                Val = val;
                //				Interlocked.Exchange(ref this.Val, Val);
            }

            /// <summary>
            /// Returns a String that represents the current Object.
            /// </summary>
            /// <returns>A string with identifying information about this class</returns>
            public override string ToString()
            {
                return String.Format(
                    "<stat Key=\"{0}\" Value=\"{1}\" LastModified=\"{2:u}\" LastModified=\"{3}\"/>",
                    Key, Val, LastModified, tcLastModified);
            }
        }

        #endregion

        #region Nested type: Vs

        /// <summary>
        /// configuration data about the DbgMutex class
        /// </summary>
        public static class Vs
        {
            /// <summary>
            /// A instance of the stats class.  This is typically used for debugging
            /// </summary>
            public static readonly Stats statsDebug = new Stats();
        }

        #endregion
    }
}