using System.Text.Json;

namespace RevolutionaryStuff.Core;

public static partial class JsonHelpers
{
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
}
