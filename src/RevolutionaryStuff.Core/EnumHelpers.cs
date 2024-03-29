﻿using System.Runtime.Serialization;

namespace RevolutionaryStuff.Core;

public static class EnumHelpers
{
    public static T Random<T>(Random r = null) where T : Enum
    {
        var a = Enum.GetValues(typeof(T));
        var n = (r ?? Stuff.Random).Next(a.Length);
        return (T)a.GetValue(n);
    }

    public static string EnumWithEnumMemberValuesToString<TEnum>(this TEnum e) where TEnum : Enum
    {
        var em = e.GetCustomAttribute<EnumMemberAttribute>();
        var sval = em?.Value ?? e.ToString();
        return sval;
    }
}
