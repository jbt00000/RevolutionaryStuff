using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace RevolutionaryStuff.AspNetCore;

public static class HttpHelpers
{
    extension(HttpRequest source)
    {
        public string GetStringFromFormOrQuery(string key, string missing = null)
        {
            string s = null;
            if (key != null)
            {
                if (!WebHelpers.Methods.IsGetOrHead(source.Method) && source.HasFormContentType && source.Form != null && source.Form.ContainsKey(key))
                {
                    s = StringHelpers.TrimOrNull(source.Form[key].FirstOrDefault());
                }
                if (s == null && source.Query != null && source.Query.ContainsKey(key))
                {
                    s = source.Query.GetString(key);
                }
            }
            return s ?? missing;
        }
        public string GetClientIp()
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
            if (source.Headers.TryGetValue("X-Forwarded-For", out var values))
            {
                return GetIp(values);
            }

            if (source.Headers.TryGetValue("X-Azure-ClientIP", out values))
            {
                return GetIp(values);
            }

            var ipaddr = source.HttpContext.Connection.RemoteIpAddress;
            return ipaddr != null ? ipaddr.ToString() : GetIp(values);
        }
    }



    extension(IHeaderDictionary sources)
    {
        public string GetString(string key, string missing = null)
                => sources[key].FirstOrDefault()?.TrimOrNull() ?? missing;
        public int? GetNullableInt32(string key, int? defaultValue = null)
                => int.TryParse(sources.GetString(key), out var result) ? result : defaultValue;
        public double? GetNullableDouble(string key, double? defaultValue = null)
                => double.TryParse(sources.GetString(key), out var result) ? result : defaultValue;
        public bool GetBool(string key, bool defaultValue = false)
                => Parse.ParseBool(sources.GetString(key), defaultValue);
        public bool? GetNullableBool(string key, bool? defaultValue = null)
                => Parse.ParseNullableBool(sources.GetString(key), defaultValue);
    }









    extension(IQueryCollection sources)
    {
        public string GetString(string key, string missing = null)
                => sources[key].FirstOrDefault()?.TrimOrNull() ?? missing;
        public int? GetNullableInt32(string key, int? defaultValue = null)
                => int.TryParse(sources.GetString(key), out var result) ? result : defaultValue;
        public double? GetNullableDouble(string key, double? defaultValue = null)
                => double.TryParse(sources.GetString(key), out var result) ? result : defaultValue;
        public bool GetBool(string key, bool defaultValue = false)
                => Parse.ParseBool(sources.GetString(key), defaultValue);
        public bool? GetNullableBool(string key, bool? defaultValue = null)
                => Parse.ParseNullableBool(sources.GetString(key), defaultValue);
    }








}
