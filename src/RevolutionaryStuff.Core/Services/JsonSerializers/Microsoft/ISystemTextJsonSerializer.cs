using System.Text.Json;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;

public interface ISystemTextJsonSerializer : IJsonSerializer
{
    JsonElement ToJsonElement(object o);
}

