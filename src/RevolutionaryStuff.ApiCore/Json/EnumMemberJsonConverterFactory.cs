using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.ApiCore.Json;

public class EnumMemberJsonConverterFactory(bool _RequiresJsonConverterAttribute, bool _IgnoreCase = true) : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        var type = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;

        if (type.IsEnum)
        {
            if (!_RequiresJsonConverterAttribute)
                return true;
            var jca = type.GetCustomAttribute<JsonConverterAttribute>();
            if (jca?.ConverterType == typeof(JsonStringEnumConverter))
                return true;
        }
        return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var nullableUnderlyingType = Nullable.GetUnderlyingType(typeToConvert);
        var type = nullableUnderlyingType ?? typeToConvert;
        var converterType = (nullableUnderlyingType == null ? typeof(EnumMemberJsonConverter<>) : typeof(NullableEnumMemberJsonConverter<>)).MakeGenericType(type);
        return (JsonConverter)Activator.CreateInstance(converterType, [_IgnoreCase])!;
    }
}
