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

    extension(Stream source)
    {
        public async Task<T> FromJsonStreamAsync<T>()
        {
            Requires.ReadableStreamArg(source);
            var json = await source.ReadToEndAsync();
            return FromJson<T>(json);
        }
        [Obsolete("Use FromJsonStreamAsync instead when possible", false)]
        public T FromJsonStream<T>()
        {
            Requires.ReadableStreamArg(source);
            var json = source.ReadToEnd();
            return FromJson<T>(json);
        }
    }



    extension(PropertyInfo source)
    {
        public bool HasJsonIgnoreAttribute()
                => source.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>() != null;
    }

    extension(MemberInfo source)
    {
        public string GetJsonPropertyName()
                => source.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>()?.Name
                ?? source.Name;
    }
}
