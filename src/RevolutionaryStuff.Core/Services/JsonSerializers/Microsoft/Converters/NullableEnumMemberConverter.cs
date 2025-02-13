using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;

internal class NullableEnumMemberConverter<TEnum> : JsonConverter<TEnum?> where TEnum : struct
{
    public bool SerializeEnumAsString { get; set; } = true;

    public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        TEnum ret = default;
        if (reader.TokenType == JsonTokenType.String)
        {
            var enumString = reader.GetString();
            ret = (TEnum)(object)Parse.ParseEnumWithEnumMemberValues(typeof(TEnum), enumString);
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            var enumLong = reader.GetInt64();
            ret = (TEnum)Enum.ToObject(typeToConvert, enumLong);
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            Stuff.NoOp();
        }
        return ret;
    }

    public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else if (SerializeEnumAsString)
        {
            var sval = ((Enum)(object)value.Value).EnumWithEnumMemberValuesToString();
            writer.WriteStringValue(sval);
        }
        else
        {
            writer.WriteNumberValue((long)(object)value.Value);
        }
    }
}
