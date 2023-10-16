using System.Text.Json;

namespace RevolutionaryStuff.Core;
public static partial class JsonHelpers
{
    public static string GetString(this IDictionary<string, JsonElement> extensionData, string key, string missing = default)
        => extensionData != null && key != null && extensionData.TryGetValue(key, out var je) ? je.GetString() : missing;

    public static int GetInt(this IDictionary<string, JsonElement> extensionData, string key, int missing = default)
        => extensionData != null && key != null && extensionData.TryGetValue(key, out var je) ? je.GetInt32() : missing;

    public static string ToMicrosoftJson(object o)
        => Services.JsonSerializers.Microsoft.DefaultJsonSerializer.Instance.ToJson(o);

    public static T FromMicrosoftJson<T>(string json)
        => Services.JsonSerializers.Microsoft.DefaultJsonSerializer.Instance.FromJson<T>(json);

    public static JsonElement ToJsonElement(object o)
        => JsonDocument.Parse(ToMicrosoftJson(o)).RootElement;

    public static T FromJsonElement<T>(this JsonElement jsonElement)
        => FromMicrosoftJson<T>(jsonElement.GetRawText());
}
