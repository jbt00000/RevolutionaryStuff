using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;

public class NullableEnumMemberConverterFactory : JsonConverterFactory
{
    public static readonly JsonConverter Instance = new NullableEnumMemberConverterFactory();

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsNullableEnum();

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var enumType = Nullable.GetUnderlyingType(typeToConvert);
        var converterType = typeof(NullableEnumMemberConverter<>).MakeGenericType(enumType);
        return (JsonConverter)Activator.CreateInstance(converterType);
    }

    private NullableEnumMemberConverterFactory()
    { }
}
