using System.Collections;
using System.Text;
using System.Web;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides formatting helpers to convert structured items to strings
/// </summary>
public static class Format
{
    /// <summary>
    /// Encodes key-value pairs as a URL query string.
    /// </summary>
    /// <param name="datas">The key-value pairs to encode. A <see langword="null"/> value produces an empty string.</param>
    /// <returns>The encoded query string, or an empty string when <paramref name="datas"/> is <see langword="null"/>.</returns>
    public static string UrlEncode(this IEnumerable<KeyValuePair<string, string>> datas)
    {
        if (datas == null) return "";
        var sb = new StringBuilder();
        var x = 0;
        foreach (var kvp in datas)
        {
            if (x++ > 0)
            {
                sb.Append("&");
            }
            sb.Append(Uri.EscapeDataString(kvp.Key));
            if (kvp.Value != null)
            {
                sb.Append("=");
                sb.Append(HttpUtility.UrlEncode(kvp.Value));
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Joins the items in an enumerable, formatting each item with a composite format string.
    /// </summary>
    /// <param name="e">The items to join. A <see langword="null"/> enumerable produces an empty string.</param>
    /// <param name="separator">The text between items; defaults to an empty string.</param>
    /// <param name="format">The composite format string for each item; defaults to <c>{0}</c>.</param>
    /// <returns>The formatted items joined by <paramref name="separator"/>.</returns>
    public static string Join(this IEnumerable e, string separator = "", string format = "{0}")
    {
        return Join(e, separator, (a, b) => string.Format(format, a, b));
    }

    /// <summary>
    /// Joins the items in an enumerable, formatting each item with a callback.
    /// </summary>
    /// <param name="e">The items to join. A <see langword="null"/> enumerable produces an empty string.</param>
    /// <param name="separator">The text between items. A <see langword="null"/> separator is omitted.</param>
    /// <param name="formatter">A callback that receives each item and its zero-based index.</param>
    /// <returns>The formatted items joined by <paramref name="separator"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is <see langword="null"/>.</exception>
    public static string Join(this IEnumerable e, string separator, Func<object, int, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        if (null == e) return "";
        var sb = new StringBuilder();
        var x = 0;
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

    /// <summary>
    /// Formats a <see cref="DateTime"/> as a SQL-style timestamp, optionally surrounded by single quotes.
    /// </summary>
    /// <param name="dt">The date and time to format.</param>
    /// <param name="quote">If <see langword="true"/>, surrounds the timestamp with single quotes.</param>
    /// <returns>The timestamp formatted as <c>yyyy-MM-ddTHH:mm:ss.fff</c>, optionally quoted.</returns>
    public static string ToSqlString(this DateTime dt, bool quote = false)
    {
        var s = $"{dt:yyyy-MM-ddTHH:mm:ss.fff}";
        if (quote) s = "'" + s + "'";
        return s;
    }

    /// <summary>
    /// Escapes a string for use as a SQL literal, optionally surrounding it with single quotes.
    /// </summary>
    /// <param name="s">The string to format. A <see langword="null"/> string is returned as the SQL token <c>null</c>.</param>
    /// <param name="quote">If <see langword="true"/>, surrounds a non-null value with single quotes.</param>
    /// <returns>The SQL literal representation of the string.</returns>
    public static string ToSqlString(this string s, bool quote = false)
    {
        if (s == null) return "null";
        s = s.Replace("'", "''");
        if (quote) s = "'" + s + "'";
        return s;
    }
}
