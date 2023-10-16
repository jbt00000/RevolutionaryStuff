using RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Core.ApplicationParts;

public abstract class JsonSerializable : IJsonSerializable
{
    public static IJsonSerializer Serializer { get; set; } = DefaultJsonSerializer.Instance;

    public string ToJson()
        => Serializer.ToJson(this);

    public static TEntity? FromJson<TEntity>(string json)
        where TEntity : JsonSerializable
        => Serializer.FromJson<TEntity>(json);
}
