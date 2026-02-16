using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;

public class SystemTextJsonSerializer(JsonSerializerOptions MyJsonSerializerOptions) : ISystemTextJsonSerializer
{
    public static readonly IEnumerable<JsonConverter> DefaultConverters =
    [
        EnumMemberConverterFactory.Instance,
            NullableEnumMemberConverterFactory.Instance,
            UndefinedJsonElementConverter.Instance
    ];

    public static readonly JsonSerializerOptions DefaultJsonSerializationSettings;

    static SystemTextJsonSerializer()
    {
        DefaultJsonSerializationSettings = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        DefaultJsonSerializationSettings.Converters.FluentAddRange(DefaultConverters);
    }

    public static readonly ISystemTextJsonSerializer Instance = new SystemTextJsonSerializer(null);

    JsonElement ISystemTextJsonSerializer.ToJsonElement(object o)
        => JsonSerializer.SerializeToElement(o, MyJsonSerializerOptions ?? DefaultJsonSerializationSettings);

    string IJsonSerializer.ToJson(object o)
        => JsonSerializer.Serialize(o, MyJsonSerializerOptions ?? DefaultJsonSerializationSettings);

    object IJsonSerializer.FromJson(string json, Type t)
        => JsonSerializer.Deserialize(json, t, MyJsonSerializerOptions ?? DefaultJsonSerializationSettings);

    string IJsonSerializer.GetMemberName(MemberInfo mi)
        => mi.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? mi.Name;
}
