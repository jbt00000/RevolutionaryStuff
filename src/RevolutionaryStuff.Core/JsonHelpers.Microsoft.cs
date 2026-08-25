using System.Text.Json;

namespace RevolutionaryStuff.Core;

public static partial class JsonHelpers
{
    public static string GetStringPropertyVal(this JsonElement jel, string propertyName, string fallback = null)
        => jel.TryGetProperty(propertyName, out var el) ? el.GetString() : fallback;

    public static string GetString(this IDictionary<string, JsonElement> extensionData, string key, string missing = default)
        => extensionData != null && key != null && extensionData.TryGetValue(key, out var je) ? je.GetString() : missing;

    public static int GetInt(this IDictionary<string, JsonElement> extensionData, string key, int missing = default)
        => extensionData != null && key != null && extensionData.TryGetValue(key, out var je) ? je.GetInt32() : missing;

    public static string ToMicrosoftJson(object o)
        => Services.JsonSerializers.Microsoft.SystemTextJsonSerializer.Instance.ToJson(o);

    public static T FromMicrosoftJson<T>(string json)
        => Services.JsonSerializers.Microsoft.SystemTextJsonSerializer.Instance.FromJson<T>(json);


    private static readonly JsonDocumentOptions DefaultJsonDocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip
    };

    public static JsonElement ToJsonElement(string json)
        => ToJsonElement(json, DefaultJsonDocumentOptions);

    public static JsonElement ToJsonElement(string json, JsonDocumentOptions options)
        => JsonDocument.Parse(json, new() { CommentHandling = JsonCommentHandling.Skip }).RootElement;

    public static JsonElement ToJsonElement(object o)
        => JsonDocument.Parse(ToMicrosoftJson(o)).RootElement;

    public static T FromJsonElement<T>(this JsonElement jsonElement)
        => FromMicrosoftJson<T>(jsonElement.GetRawText());

    public static string NullSafeGetJsonPropertyAsString(this IDictionary<string, JsonElement> additionalData, string key, string fallback = default)
    {
        return additionalData == null || additionalData.TryGetValue(key, out var je) == false ? fallback : je.GetString();
    }

    public record ToPocoSettings
    {
        public bool TreatValueKindUndefinedAsNull { get; init; } = true;
        public bool TreatValueKindArrayAsList { get; init; } = true;
        public bool TreatValueKindObjectAsDictionary { get; init; } = true;
        public bool ConvertStringToGuid { get; init; } = true;
        public bool ConvertStringToDateTimeOffset { get; init; } = true;
        public bool ConvertStringToDateTime { get; init; } = true;
        public bool ConvertStringToUri { get; init; } = false;
        public bool ConvertStringToDateOnly { get; init; } = false;
        public bool ConvertStringToTimeOnly { get; init; } = false;
        public bool ConvertStringToTimeSpan { get; init; } = false;
        public bool ConvertStringToVersion { get; init; } = false;
        public bool ConvertStringToBytesFromBase64 { get; init; } = false;

        /// <summary>
        /// The comparer used for keys in dictionaries created for JSON objects. Defaults to the ordinal (case-sensitive) comparer.
        /// </summary>
        public IEqualityComparer<string> DictionaryComparer { get; init; } = StringComparer.Ordinal;

        public NumericConversionStrategyEnum NumericConversionStrategy { get; init; } = NumericConversionStrategyEnum.PopularTypes;

        public enum NumericConversionStrategyEnum
        {
            PopularTypes,
            FitToSize,
        }
    }

    private static readonly ToPocoSettings DefaultToPocoSettings = new();

    public static object ToPoco(this JsonElement jel, ToPocoSettings settings = default)
        => ToPoco((object) jel, settings);

    public static object ToPoco(object o, ToPocoSettings settings = default)
    {
        settings ??= DefaultToPocoSettings;
        if (o is JsonElement jel)
        {
            switch (jel.ValueKind)
            {
                case JsonValueKind.Object:
                    if (settings.TreatValueKindObjectAsDictionary)
                    {
                        var dictionary = new Dictionary<string, object>(settings.DictionaryComparer);
                        foreach (var property in jel.EnumerateObject())
                        {
                            dictionary[property.Name] = ToPoco(property.Value, settings);
                        }
                        return dictionary;
                    }
                    break;
                case JsonValueKind.Array:
                    if (settings.TreatValueKindArrayAsList)
                    {
                        var array = new List<object>();
                        jel.EnumerateArray().ForEach(ja => array.Add(ToPoco(ja, settings)));
                        return array;
                    }
                    break;
                case JsonValueKind.String:
                    if (settings.ConvertStringToGuid && jel.TryGetGuid(out var @guid)) return @guid;
                    if (settings.ConvertStringToDateTimeOffset && jel.TryGetDateTimeOffset(out var @dateTimeOffset)) return @dateTimeOffset;
                    if (settings.ConvertStringToDateTime && jel.TryGetDateTime(out var @dateTime)) return @dateTime;
                    if (settings.ConvertStringToBytesFromBase64 && jel.TryGetBytesFromBase64(out var bytes)) return bytes;

                    var s = jel.GetString();
                    if (s != null)
                    {
                        if (settings.ConvertStringToUri && Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var uri)) return uri;
                        if (settings.ConvertStringToDateOnly && DateOnly.TryParse(s, out var dateOnly)) return dateOnly;
                        if (settings.ConvertStringToTimeOnly && TimeOnly.TryParse(s, out var timeOnly)) return timeOnly;
                        if (settings.ConvertStringToTimeSpan && TimeSpan.TryParse(s, out var timeSpan)) return timeSpan;
                        if (settings.ConvertStringToVersion && Version.TryParse(s, out var version)) return version;
                    }

                    return s;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Undefined:
                    return settings.TreatValueKindUndefinedAsNull ? null : o;
                case JsonValueKind.Number:
                    switch (settings.NumericConversionStrategy)
                    {
                        case ToPocoSettings.NumericConversionStrategyEnum.PopularTypes:
                            {
                                if (jel.TryGetInt32(out var @int)) return @int;
                                if (jel.TryGetInt64(out var @long)) return @long;
                                if (jel.TryGetDouble(out var @double)) return @double;
                                if (jel.TryGetDecimal(out var @decimal)) return @decimal;
                            }
                            break;
                        case ToPocoSettings.NumericConversionStrategyEnum.FitToSize:
                            {
                                if (jel.TryGetInt16(out var @short)) return @short;
                                if (jel.TryGetInt32(out var @int)) return @int;
                                if (jel.TryGetInt64(out var @long)) return @long;
                                if (jel.TryGetUInt16(out var @ushort)) return @ushort;
                                if (jel.TryGetUInt32(out var @uint)) return @uint;
                                if (jel.TryGetUInt64(out var @ulong)) return @ulong;
                                if (jel.TryGetDecimal(out var @decimal)) return @decimal;
                                if (jel.TryGetDouble(out var @double)) return @double;
                                if (jel.TryGetSingle(out var @single)) return @single;
                            }
                            break;
                        default: throw new UnexpectedSwitchValueException(settings.NumericConversionStrategy);
                    }
                    break;
            }
        }
        return o;
    }
}
