using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;

internal class EnumMemberConverter<TEnum> : JsonConverter<TEnum> where TEnum : Enum
{
    public bool SerializeEnumAsString { get; set; } = true;

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var enumString = reader.GetString();
            return Parse.ParseEnumWithEnumMemberValues<TEnum>(enumString);
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            var enumLong = reader.GetInt64();
            return (TEnum)Enum.ToObject(typeToConvert, enumLong);
        }
        return default;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (SerializeEnumAsString)
        {
            writer.WriteStringValue(value.EnumWithEnumMemberValuesToString());
        }
        else
        {
            writer.WriteNumberValue((long)(object)value);
        }
    }
}
