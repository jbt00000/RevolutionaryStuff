using RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Data.Cosmos;

public class DefaultCosmosEntitySerializer : JsonSerializer2CosmosSerializerAdaptor
{
    public DefaultCosmosEntitySerializer(IJsonSerializer? jsonSerializer = null)
        : base(jsonSerializer ?? SystemTextJsonSerializer.Instance)
    { }
}

