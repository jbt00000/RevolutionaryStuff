using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.ApiCore.Json;

public class EnumMemberJsonConverter<T>(bool _IgnoreCase = true) : JsonConverter<T> where T : struct, Enum
{
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

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var field = typeof(T).GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();

        writer.WriteStringValue(attribute?.Value ?? value.ToString());
    }
}
