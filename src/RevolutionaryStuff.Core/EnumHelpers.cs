using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core;

public static class EnumHelpers
{
    public static bool In<TEnum>(this TEnum o, params TEnum[] vals)
        where TEnum : Enum
        => vals != null && vals.Contains(o);

    public static T Random<T>(Random r = null) where T : Enum
    {
        var a = Enum.GetValues(typeof(T));
        var n = (r ?? Stuff.Random).Next(a.Length);
        return (T)a.GetValue(n);
    }

    public static string EnumWithEnumMemberValuesToString<TEnum>(this TEnum e) where TEnum : Enum
    {
        var enumMemberValue = e.GetCustomAttribute<EnumMemberAttribute>()?.Value;
        var jsonStringEnumMemberName = e.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name;
        return enumMemberValue ?? jsonStringEnumMemberName ?? e.ToString();
    }

    public static bool Any<TEnum>(TEnum e, params Enum[] values)
    where TEnum : struct, Enum
    {
        if (values == null || values.Length == 0) return false;
        foreach (var v in values)
        {
            if (e.Equals(v)) return true;
        }
        return false;
    }
    public static bool Any<TEnum>(TEnum? e, params Enum[] values)
        where TEnum : struct, Enum
    {
        if (e == null || values == null || values.Length == 0) return false;
        foreach (var v in values)
        {
            if (e.Equals(v)) return true;
        }
        return false;
    }
}
