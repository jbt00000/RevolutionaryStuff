using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;

/// <summary>
/// JSON converter for enums that respects the <see cref="System.Runtime.Serialization.EnumMemberAttribute"/> for serialization.
/// Supports both value serialization and property name serialization (for dictionary keys).
/// Can serialize enums as strings or numbers based on <see cref="SerializeEnumAsString"/>.
/// </summary>
/// <typeparam name="TEnum">The enum type to convert.</typeparam>
internal class EnumMemberConverter<TEnum> : JsonConverter<TEnum> where TEnum : Enum
{
    /// <summary>
    /// Gets or sets whether to serialize enums as strings (true) or numbers (false).
    /// Default is true.
    /// </summary>
    public bool SerializeEnumAsString { get; set; } = true;

    /// <summary>
    /// Reads and converts the JSON to an enum value, respecting the <see cref="System.Runtime.Serialization.EnumMemberAttribute"/>.
    /// Supports both string and numeric JSON representations.
    /// </summary>
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        TEnum ret = default;
        if (reader.TokenType == JsonTokenType.String)
        {
            var enumString = reader.GetString();
            ret = Parse.ParseEnumWithEnumMemberValues<TEnum>(enumString);
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            var enumLong = reader.GetInt64();
            ret = (TEnum)Enum.ToObject(typeToConvert, enumLong);
        }
        return ret;
    }

    /// <summary>
    /// Writes the enum value as JSON, using the <see cref="System.Runtime.Serialization.EnumMemberAttribute"/> value if present.
    /// Serializes as string or number based on <see cref="SerializeEnumAsString"/>.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (SerializeEnumAsString)
        {
            writer.WriteStringValue(value.EnumWithEnumMemberValuesToString());
        }
        else
        {
            var nVal = Convert.ToInt64(value);
            writer.WriteNumberValue(nVal);
        }
    }

    /// <summary>
    /// Reads and converts a JSON property name to an enum value, respecting the <see cref="System.Runtime.Serialization.EnumMemberAttribute"/>.
    /// Used when the enum is a dictionary key.
    /// </summary>
    public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();
        return Parse.ParseEnumWithEnumMemberValues<TEnum>(enumString);
    }

    /// <summary>
    /// Writes the enum value as a JSON property name, using the <see cref="System.Runtime.Serialization.EnumMemberAttribute"/> value if present.
    /// Used when the enum is a dictionary key.
    /// Always serializes as string regardless of <see cref="SerializeEnumAsString"/> setting (property names must be strings).
    /// </summary>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.EnumWithEnumMemberValuesToString());
    }
}
