using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RevolutionaryStuff.AspNetCore;

public static class TempDataExtensions
{
    extension(ITempDataDictionary sources)
    {
        public void SetAsJsonValue<T>(string key, T value) where T : class
                => sources[key] = JsonHelpers.ToJson(value);
        public T GetFromJsonValue<T>(string key, T missing = default) where T : class
        {
            sources.TryGetValue(key, out var o);
            return o is not string json ? missing : JsonHelpers.FromJson<T>(json);
        }
    }


}
