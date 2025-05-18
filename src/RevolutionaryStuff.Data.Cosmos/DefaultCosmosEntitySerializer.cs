using RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Data.Cosmos;

public class DefaultCosmosEntitySerializer(IJsonSerializer? jsonSerializer = null, string? TypeDiscriminatorPropertyNameToDedupe = "$type")
    : JsonSerializer2CosmosSerializerAdaptor(jsonSerializer ?? SystemTextJsonSerializer.Instance, TypeDiscriminatorPropertyNameToDedupe)
{ }
