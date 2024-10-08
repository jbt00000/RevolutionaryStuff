﻿using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;

public class SystemTextJsonSerializer : IJsonSerializer
{
    public static readonly IEnumerable<JsonConverter> DefaultConverters =
    [
        EnumMemberConverterFactory.Instance,
        NullableEnumMemberConverterFactory.Instance,
        UndefinedJsonElementConverter.Instance
    ];

    public static readonly JsonSerializerOptions MyJsonSerializationSettings;

    static SystemTextJsonSerializer()
    {
        MyJsonSerializationSettings = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        MyJsonSerializationSettings.Converters.FluentAddRange(DefaultConverters);
    }

    public static readonly IJsonSerializer Instance = new SystemTextJsonSerializer();

    string IJsonSerializer.ToJson(object o)
        => JsonSerializer.Serialize(o, MyJsonSerializationSettings);

    object IJsonSerializer.FromJson(string json, Type t)
        => JsonSerializer.Deserialize(json, t, MyJsonSerializationSettings);

    string IJsonSerializer.GetMemberName(MemberInfo mi)
        => mi.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? mi.Name;
}
