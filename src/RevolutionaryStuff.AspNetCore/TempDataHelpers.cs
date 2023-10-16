using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RevolutionaryStuff.AspNetCore;

public static class TempDataExtensions
{
    public static void SetAsJsonValue<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        => tempData[key] = JsonHelpers.ToJson(value);

    public static T GetFromJsonValue<T>(this ITempDataDictionary tempData, string key, T missing = default) where T : class
    {
        tempData.TryGetValue(key, out var o);
        return o is not string json ? missing : JsonHelpers.FromJson<T>(json);
    }
}
