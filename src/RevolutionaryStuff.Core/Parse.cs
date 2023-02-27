using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core;

public static class Parse
{
    public static TEnum? ParseNullableEnumWithEnumMemberValues<TEnum>(string val, bool caseSensitive = false, TEnum? missing = null) where TEnum : struct
    {
        if (!string.IsNullOrEmpty(val))
        {
            try
            {
                var ret = ParseEnumWithEnumMemberValues(typeof(TEnum), val, caseSensitive, null);
                if (ret != null)
                {
                    return (TEnum)(object)ret;
                }
            }
            catch (Exception)
            { }
        }
        return missing;
    }

    public static TEnum ParseEnumWithEnumMemberValues<TEnum>(string val, bool caseSensitive = false, TEnum missing = default) where TEnum : Enum
        => (TEnum)ParseEnumWithEnumMemberValues(typeof(TEnum), val, caseSensitive, missing);

    public static Enum ParseEnumWithEnumMemberValues(Type t, string val, bool caseSensitive = false, Enum missing = default)
    {
        var d = Cache.DataCacher.FindOrCreateValue<IDictionary<string, object>>(
            Cache.CreateKey(t, caseSensitive),
            () =>
            {
                var z = caseSensitive ?
                    new Dictionary<string, object>() :
                    new Dictionary<string, object>(Comparers.CaseInsensitiveStringComparer);
                foreach (var v in Enum.GetValues(t))
                {
                    var mi = t.GetMember(v.ToString()).First();
                    var em = mi.GetCustomAttribute<EnumMemberAttribute>();
                    if (em != null)
                    {
                        z[em.Value] = v;
                    }
                    else
                    {
                        z[mi.Name] = v;
                    }
                }
                return z;
            }
            );
        if (!string.IsNullOrEmpty(val))
        {
            if (d.ContainsKey(val)) return (Enum)d[val];
        }
        return missing;
    }

    public static T? ParseNullableEnum<T>(string s, T? fallback = null) where T : struct
    {
        if (!string.IsNullOrEmpty(s))
        {
            try
            {
                return (T)Enum.Parse(typeof(T), s, true);
            }
            catch (Exception)
            { }
        }
        return fallback;
    }

    public static int? ParseNullableInt32(string s, int? fallback = null)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (int.TryParse(s, out var i)) return i;
        }
        return fallback;
    }

    public static long? ParseNullableInt64(string s, long? fallback = null)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (long.TryParse(s, out var i)) return i;
        }
        return fallback;
    }

    public static DateTime? ParseNullableDateTime(string s, DateTime? fallback = null)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (DateTime.TryParse(s, out var dt)) return dt;
        }
        return fallback;
    }

    public static DateTimeOffset? ParseNullableDateTimeOffset(string s, DateTimeOffset? fallback = null)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (DateTimeOffset.TryParse(s, out var dto)) return dto;
            if (s.EndsWith('Z') || s.EndsWith('z'))
            {
                if (DateTimeOffset.TryParse(s[..^1], out dto))
                {
                    return dto;
                }
            }
            if (DateTime.TryParse(s, out var dt)) return new DateTimeOffset(dt);
        }
        return fallback;
    }

    public static DateTime ParseYYYYMMDD(string date)
    {
        date = date.Trim();
        return new DateTime(
            int.Parse(date[..4]),
            int.Parse(date.Substring(4, 2)),
            int.Parse(date.Substring(6, 2)));
    }

    public static double? ParseNullableDouble(string s, double? fallback = null)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (double.TryParse(s, out var i)) return i;
        }
        return fallback;
    }

    private static readonly Regex BoolTrueExpr = new("^\\s*(true|1)\\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex BoolFalseExpr = new("^\\s*(false|0)\\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    public static bool? ParseNullableBool(string sv, bool? fallback = null)
    {
        return TryParseBool(sv, out var v) ? v : fallback;
    }

    public static bool TryParseBool(string s, out bool v)
    {
        v = false;
        try
        {
            if (null != s && s.Length > 0)
            {
                if (BoolTrueExpr.IsMatch(s))
                {
                    v = true;
                    return true;
                }

                if (BoolFalseExpr.IsMatch(s))
                {
                    v = false;
                    return true;
                }
            }
        }
        catch (Exception)
        { }
        return false;
    }

    /// <summary>
    /// Parse an Int64.  Allows for missing value.  Empty strings return default.
    /// </summary>
    /// <param name="s">String that contains the number</param>
    /// <param name="fallback">Value to return if there is a parsing error</param>
    /// <returns>Parsed Value OR Default </returns>
    public static long ParseInt64(string s, long fallback = 0)
    {
        if (!long.TryParse(s, out var ret))
        {
            ret = fallback;
        }
        return ret;
    }

    public static Uri ParseUri(string s, UriKind kind = UriKind.Absolute)
    {
        return Uri.TryCreate(s, kind, out var u) ? u : null;
    }

    /// <summary>
    /// Parse an double.  Allows for missing value.  Empty strings return default.
    /// </summary>
    /// <param name="s">String that contains the number</param>
    /// <param name="fallback">Value to return if there is a parsing error</param>
    /// <returns>Parsed Value OR Default </returns>
    public static double ParseDouble(string s, double fallback = 0)
    {
        if (!double.TryParse(s, out var ret))
        {
            ret = fallback;
        }
        return ret;
    }

    /// <summary>
    /// Parse an Int32.  Allows for missing value.  Empty strings return default.
    /// </summary>
    /// <param name="s">String that contains the number</param>
    /// <param name="default">Value to return if there is a parsing error</param>
    /// <returns>Parsed Value OR Default </returns>
    public static int ParseInt32(string s, int fallback = 0)
    {
        if (!int.TryParse(s, out var ret))
        {
            ret = fallback;
        }
        return ret;
    }

    /// <summary>
    /// Parse an Int32.  Allows for missing value.  Empty strings return default.
    /// </summary>
    /// <param name="s">String that contains the number</param>
    /// <param name="default">Value to return if there is a parsing error</param>
    /// <returns>Parsed Value OR Default </returns>
    public static bool ParseBool(string s, bool fallback = false)
    {
        return TryParseBool(s, out var v) ? v : fallback;
    }

    /// <summary>
    /// Parse a TimeSpan.  Allows for missing value.  
    /// </summary>
    /// <param name="s">A string that might be a timespan</param>
    /// <param name="fallback">The value to use if the conversion fails</param>
    /// <returns>Parsed value or default</returns>
    public static TimeSpan ParseTimeSpan(string s, TimeSpan fallback)
    {
        return TimeSpan.TryParse(s, out var ts) ? ts : fallback;
    }

    public static TEnum ParseEnum<TEnum>(string val, TEnum? fallback = null) where TEnum : struct
    {
        if (val != null && val.Length > 0)
        {
            try
            {
                return (TEnum)Enum.Parse(typeof(TEnum), val, true);
            }
            catch (Exception)
            { }
        }
        return fallback == null
            ? throw new ArgumentOutOfRangeException(nameof(val), $"{val} cannot be cast to {typeof(TEnum)}")
            : fallback.Value;
    }

    /// <summary>
    /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
    /// If the conversion cannot take place, use the fallback
    /// </summary>
    /// <param name="enumType">The Type of the enumeration</param>
    /// <param name="val">A string containing the name of value to convert</param>
    /// <param name="fallback">The value to use if the conversion fails</param>
    /// <returns>The converted value when possible, else fallback</returns>
    public static Enum ParseEnum(Type enumType, string val, Enum fallback)
    {
        return ParseEnum(enumType, val, true, fallback);
    }

    /// <summary>
    /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
    /// If the conversion cannot take place, use the fallback
    /// </summary>
    /// <param name="enumType">The Type of the enumeration</param>
    /// <param name="val">A string containing the name of value to convert</param>
    /// <param name="ignoreCase">If true, ignore case</param>
    /// <param name="fallback">The value to use if the conversion fails</param>
    /// <returns>The converted value when possible, else fallback</returns>
    public static Enum ParseEnum(Type enumType, string val, bool ignoreCase, Enum fallback)
    {
        if (val == null || val.Length == 0) return fallback;
        try
        {
            return (Enum)Enum.Parse(enumType, val, ignoreCase);
        }
        catch (Exception)
        {
            return fallback;
        }
    }
}
