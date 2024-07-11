using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;

public class UndefinedJsonElementConverter : JsonConverter<JsonElement>
{
    public static readonly JsonConverter Instance = new UndefinedJsonElementConverter();

    public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonDocument.ParseValue(ref reader).RootElement;
    }

    public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
    {
        if (value.ValueKind != JsonValueKind.Undefined)
        {
            value.WriteTo(writer);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
