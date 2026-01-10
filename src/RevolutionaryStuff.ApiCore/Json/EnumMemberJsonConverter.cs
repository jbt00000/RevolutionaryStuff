using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.ApiCore.Json;

/// <summary>
/// JSON converter for enums that respects the <see cref="EnumMemberAttribute"/> for serialization.
/// Supports both value serialization and property name serialization (for dictionary keys).
/// </summary>
/// <typeparam name="T">The enum type to convert.</typeparam>
/// <param name="_IgnoreCase">Whether to ignore case when parsing enum values.</param>
public class EnumMemberJsonConverter<T>(bool _IgnoreCase = true) : JsonConverter<T> where T : struct, Enum
{
    /// <summary>
    /// Reads and converts the JSON to an enum value, respecting the <see cref="EnumMemberAttribute"/>.
    /// </summary>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();
        if (enumString == null) throw new JsonException();

        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
            if (0 == string.Compare(attribute?.Value, enumString, _IgnoreCase))
            {
                return (T)field.GetValue(null)!;
            }
        }

        return Enum.TryParse(enumString, ignoreCase: _IgnoreCase, out T result)
            ? result
            : throw new JsonException($"Invalid value '{enumString}' for enum {typeof(T).Name}");
    }

    /// <summary>
    /// Writes the enum value as JSON, using the <see cref="EnumMemberAttribute"/> value if present.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var field = typeof(T).GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();

        writer.WriteStringValue(attribute?.Value ?? value.ToString());
    }

    /// <summary>
    /// Reads and converts a JSON property name to an enum value, respecting the <see cref="EnumMemberAttribute"/>.
    /// Used when the enum is a dictionary key.
    /// </summary>
    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();
        if (enumString == null) throw new JsonException();

        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
            if (0 == string.Compare(attribute?.Value, enumString, _IgnoreCase))
            {
                return (T)field.GetValue(null)!;
            }
        }

        return Enum.TryParse(enumString, ignoreCase: _IgnoreCase, out T result)
            ? result
            : throw new JsonException($"Invalid property name '{enumString}' for enum {typeof(T).Name}");
    }

    /// <summary>
    /// Writes the enum value as a JSON property name, using the <see cref="EnumMemberAttribute"/> value if present.
    /// Used when the enum is a dictionary key.
    /// </summary>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var field = typeof(T).GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();

        writer.WritePropertyName(attribute?.Value ?? value.ToString());
    }
}
