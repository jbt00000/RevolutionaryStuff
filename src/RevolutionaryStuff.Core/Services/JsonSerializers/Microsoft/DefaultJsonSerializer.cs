using System.Text.Json;
using System.Text.Json.Serialization;
using RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;

public class DefaultJsonSerializer : IJsonSerializer
{
    public static readonly IEnumerable<JsonConverter> DefaultConverters = new JsonConverter[]
    {
        EnumMemberConverterFactory.Instance,
        NullableEnumMemberConverterFactory.Instance,
    };

    public static readonly IJsonSerializer Instance = new DefaultJsonSerializer();

    private readonly JsonSerializerOptions MyJsonSerializationSettings;

    private DefaultJsonSerializer()
    {
        MyJsonSerializationSettings = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        MyJsonSerializationSettings.Converters.FluentAddRange(DefaultConverters);
    }

    string IJsonSerializer.ToJson(object o)
        => JsonSerializer.Serialize(o, MyJsonSerializationSettings);

    object IJsonSerializer.FromJson(string json, Type t)
        => JsonSerializer.Deserialize(json, t, MyJsonSerializationSettings);
}
