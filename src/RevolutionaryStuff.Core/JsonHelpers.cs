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

    public static bool HasJsonIgnoreAttribute(this PropertyInfo pi)
        => pi.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>() != null
        || pi.GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>() != null;

    public static string GetJsonPropertyName(this PropertyInfo pi)
        => pi.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>()?.Name
        ?? pi.GetCustomAttribute<Newtonsoft.Json.JsonPropertyAttribute>()?.PropertyName
        ?? pi.Name;
}
