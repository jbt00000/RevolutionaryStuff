using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;

public class EnumMemberConverterFactory : JsonConverterFactory
{
    public static readonly JsonConverter Instance = new EnumMemberConverterFactory();

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumMemberConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType);
    }

    private EnumMemberConverterFactory()
    { }
}
