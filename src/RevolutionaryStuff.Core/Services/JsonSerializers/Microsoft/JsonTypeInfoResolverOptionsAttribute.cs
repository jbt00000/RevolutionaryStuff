using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class JsonTypeInfoResolverOptionsAttribute : JsonAttribute
{
    public bool RemoveWhenPolymorphic { get; set; }

    public JsonTypeInfoResolverOptionsAttribute()
    {
    }
}
