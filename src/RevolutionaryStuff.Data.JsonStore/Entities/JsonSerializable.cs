using System.Runtime.Serialization;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Data.JsonStore.Entities;

public abstract class JsonSerializable : IJsonSerializable
{
    public static IJsonSerializer Serializer { get; internal set; } = DefaultJsonSerializer.Instance;

    public string ToJson()
        => Serializer.ToJson(this);

    public static TEntity? FromJson<TEntity>(string json)
        where TEntity : JsonSerializable
        => Serializer.FromJson<TEntity>(json);
}
