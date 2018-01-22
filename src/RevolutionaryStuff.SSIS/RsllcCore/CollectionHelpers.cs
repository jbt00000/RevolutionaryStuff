using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RevolutionaryStuff.Core
{
    public static class CollectionHelpers
    {
        public static string Format(this IEnumerable e, string sep = "", string format = "{0}")
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

        public static V FindOrCreate<K, V>(this IDictionary<K, V> d, K key, Func<V> creator)
        {
            if (!d.TryGetValue(key, out V ret))
            {
                ret = creator();
                d[key] = ret;
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
    }
}
