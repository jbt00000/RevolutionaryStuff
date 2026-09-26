using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides helpers for testing, selecting, and formatting enum values.
/// </summary>
public static class EnumHelpers
{
    /// <summary>
    /// Determines whether an enum value matches any of the specified values.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="o">The value to test.</param>
    /// <param name="vals">The values to compare against.</param>
    /// <returns><see langword="true"/> if <paramref name="o"/> is in <paramref name="vals"/>; otherwise, <see langword="false"/>.</returns>
    public static bool In<TEnum>(this TEnum o, params TEnum[] vals)
        where TEnum : Enum
        => vals != null && vals.Contains(o);

    /// <summary>
    /// Selects a random value defined by an enum type.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="r">The random number generator to use, or <see langword="null"/> to use <see cref="Stuff.Random"/>.</param>
    /// <returns>A randomly selected value defined by <typeparamref name="T"/>.</returns>
    public static T Random<T>(Random r = null) where T : Enum
    {
        var a = Enum.GetValues(typeof(T));
        var n = (r ?? Stuff.Random).Next(a.Length);
        return (T)a.GetValue(n);
    }

    /// <summary>
    /// Gets the serialized name associated with an enum value, if present, or its standard enum name.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="e">The enum value to format.</param>
    /// <returns>The <see cref="EnumMemberAttribute"/> value, then the <see cref="JsonStringEnumMemberNameAttribute"/> name, or the enum value's standard name.</returns>
    public static string EnumWithEnumMemberValuesToString<TEnum>(this TEnum e) where TEnum : Enum
    {
        var enumMemberValue = e.GetCustomAttribute<EnumMemberAttribute>()?.Value;
        var jsonStringEnumMemberName = e.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name;
        return enumMemberValue ?? jsonStringEnumMemberName ?? e.ToString();
    }

    /// <summary>
    /// Determines whether an enum value equals any of the specified values.
    /// </summary>
    /// <typeparam name="TEnum">The enum value type.</typeparam>
    /// <param name="e">The value to test.</param>
    /// <param name="values">The enum values to compare against.</param>
    /// <returns><see langword="true"/> if a value equals <paramref name="e"/>; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>
    /// Determines whether a non-null enum value equals any of the specified values.
    /// </summary>
    /// <typeparam name="TEnum">The enum value type.</typeparam>
    /// <param name="e">The nullable value to test.</param>
    /// <param name="values">The enum values to compare against.</param>
    /// <returns><see langword="true"/> if <paramref name="e"/> is non-null and equals a value; otherwise, <see langword="false"/>.</returns>
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
