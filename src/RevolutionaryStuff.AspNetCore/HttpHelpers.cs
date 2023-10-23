using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

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

    public static string GetClientIp(this HttpRequest req)
    {
        static string GetIp(StringValues vals)
        {
            var ipn = vals.FirstOrDefault()?.Split(new[] { ',' }).FirstOrDefault()
                ?.Split(new[] { ':' }).FirstOrDefault();
            _ = System.Net.IPAddress.TryParse(ipn, out var ip);
            var result = ip?.ToString();
            return result;
        }
        //https://docs.microsoft.com/en-us/azure/frontdoor/front-door-http-headers-protocol
        //https://stackoverflow.com/questions/37582553/how-to-get-client-ip-address-in-azure-functions-c
        if (req.Headers.TryGetValue("X-Forwarded-For", out var values))
        {
            return GetIp(values);
        }

        if (req.Headers.TryGetValue("X-Azure-ClientIP", out values))
        {
            return GetIp(values);
        }

        var ipaddr = req.HttpContext.Connection.RemoteIpAddress;
        return ipaddr != null ? ipaddr.ToString() : GetIp(values);
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
