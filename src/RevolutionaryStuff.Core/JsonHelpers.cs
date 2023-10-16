using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Core;

public static partial class JsonHelpers
{
    public static T Clone<T>(T obj)
        => IJsonSerializer.Default.Clone(obj);
}
