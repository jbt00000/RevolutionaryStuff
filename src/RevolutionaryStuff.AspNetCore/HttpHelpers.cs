using Microsoft.AspNetCore.Http;

namespace RevolutionaryStuff.AspNetCore;

public static class HttpHelpers
{
    public static string GetStringFromFormOrQuery(this HttpRequest req, string key, string missing = null)
    {
        string s = null;
        if (key != null)
        {
            if (!WebHelpers.Methods.IsGetOrHead(req.Method) && req.HasFormContentType && req.Form != null && req.Form.ContainsKey(key))
            {
                s = StringHelpers.TrimOrNull(req.Form[key].FirstOrDefault());
            }
            if (s == null && req.Query != null && req.Query.ContainsKey(key))
            {
                s = req.Query.GetString(key);
            }
        }
        return s ?? missing;
    }

    public static string GetString(this IHeaderDictionary q, string key, string missing = null)
        => q[key].FirstOrDefault()?.TrimOrNull() ?? missing;

    public static int? GetNullableInt32(this IHeaderDictionary q, string key, int? defaultValue = null)
        => int.TryParse(q.GetString(key), out var result) ? result : defaultValue;

    public static double? GetNullableDouble(this IHeaderDictionary q, string key, double? defaultValue = null)
        => double.TryParse(q.GetString(key), out var result) ? result : defaultValue;

    public static bool GetBool(this IHeaderDictionary q, string key, bool defaultValue = false)
        => Parse.ParseBool(q.GetString(key), defaultValue);

    public static bool? GetNullableBool(this IHeaderDictionary q, string key, bool? defaultValue = null)
        => Parse.ParseNullableBool(q.GetString(key), defaultValue);

    public static string GetString(this IQueryCollection q, string key, string missing = null)
        => q[key].FirstOrDefault()?.TrimOrNull() ?? missing;

    public static int? GetNullableInt32(this IQueryCollection q, string key, int? defaultValue = null)
        => int.TryParse(q.GetString(key), out var result) ? result : defaultValue;

    public static double? GetNullableDouble(this IQueryCollection q, string key, double? defaultValue = null)
        => double.TryParse(q.GetString(key), out var result) ? result : defaultValue;

    public static bool GetBool(this IQueryCollection q, string key, bool defaultValue = false)
        => Parse.ParseBool(q.GetString(key), defaultValue);

    public static bool? GetNullableBool(this IQueryCollection q, string key, bool? defaultValue = null)
        => Parse.ParseNullableBool(q.GetString(key), defaultValue);
}
