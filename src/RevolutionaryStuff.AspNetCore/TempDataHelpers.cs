using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace RevolutionaryStuff.AspNetCore;

public static class TempDataExtensions
{
    public static JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    public static Formatting Formatting = Formatting.Indented;

    public static void SetAsJsonValue<T>(this ITempDataDictionary tempData, string key, T value) where T : class
    {
        tempData[key] = JsonConvert.SerializeObject(value, Formatting, Settings);
    }

    public static T GetFromJsonValue<T>(this ITempDataDictionary tempData, string key, T missing = default) where T : class
    {
        tempData.TryGetValue(key, out var o);
        return o is not string json ? missing : JsonConvert.DeserializeObject<T>(json, Settings);
    }
}
