using System.IO;
using System.Reflection;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Core;

public static partial class JsonHelpers
{
    public static T Clone<T>(T obj)
        => IJsonSerializer.Default.Clone(obj);

    public static string ToJson(object o)
        => IJsonSerializer.Default.ToJson(o);

    public static T FromJson<T>(string json)
        => IJsonSerializer.Default.FromJson<T>(json);

    public static async Task<T> FromJsonStreamAsync<T>(this Stream st)
    {
        Requires.ReadableStreamArg(st);
        var json = await st.ReadToEndAsync();
        return FromJson<T>(json);
    }

    [Obsolete("Use FromJsonStreamAsync instead when possible", false)]
    public static T FromJsonStream<T>(this Stream st)
    {
        Requires.ReadableStreamArg(st);
        var json = st.ReadToEnd();
        return FromJson<T>(json);
    }

    public static bool HasJsonIgnoreAttribute(this PropertyInfo pi)
        => pi.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>() != null
        || pi.GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>() != null;

    public static string GetJsonPropertyName(this MemberInfo mi)
        => mi.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>()?.Name
        ?? mi.GetCustomAttribute<Newtonsoft.Json.JsonPropertyAttribute>()?.PropertyName
        ?? mi.Name;
}
