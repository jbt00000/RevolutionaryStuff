using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RevolutionaryStuff.Core
{
    public static class Format
    {
        public static string UrlEncode(this IEnumerable<KeyValuePair<string, string>> datas)
        {
            if (datas == null) return "";
            var sb = new StringBuilder();
            int x = 0;
            foreach (var kvp in datas)
            {
                if (x++ > 0)
                {
                    sb.Append("&");
                }
                sb.Append(Uri.EscapeUriString(kvp.Key));
                if (kvp.Value != null)
                {
                    sb.Append("=");
                    sb.Append(Uri.EscapeDataString(kvp.Value));
                }
            }
            return sb.ToString();
        }

        public static string Join(this IEnumerable e, string separator = "", string format="{0}")
        {
            return Join(e, separator, (a, b) => string.Format(format, a, b));
        }

        public static string Join(this IEnumerable e, string separator, Func<object, int, string> formatter)
        {
            Requires.NonNull(formatter, nameof(formatter));
            if (null == e) return "";
            var sb = new StringBuilder();
            int x = 0;
            foreach (var item in e)
            {
                if (x > 0 && null != separator)
                {
                    sb.Append(separator);
                }
                sb.Append(formatter(item, x++));
            }
            return sb.ToString();
        }

        public static string ToSqlString(this DateTime dt, bool quote = false)
        {
            var s = string.Format("{0:yyyy-MM-ddTHH:mm:ss.fff}", dt);
            if (quote) s = "'" + s + "'";
            return s;
        }

        public static string ToSqlString(this string s, bool quote = false)
        {
            if (s == null) return "null";
            s = s.Replace("'", "''");
            if (quote) s = "'" + s + "'";
            return s;
        }
    }
}
