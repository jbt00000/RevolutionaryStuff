using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.ApiCore.Json;

public class EnumMemberJsonConverterFactory(bool _RequiresJsonConverterAttribute, bool _IgnoreCase = true) : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsEnum)
        {
            if (!_RequiresJsonConverterAttribute)
                return true;
            var jca = typeToConvert.GetCustomAttribute<JsonConverterAttribute>();
            if (jca?.ConverterType == typeof(JsonStringEnumConverter))
                return true;
        }
        return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumMemberJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, [_IgnoreCase])!;
    }
}
