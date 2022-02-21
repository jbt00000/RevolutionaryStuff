using Microsoft.AspNetCore.Http;

namespace RevolutionaryStuff.AspNetCore
{
    public static class HttpRequestHelpers
    {
        public static string GetStringFromFormOrQuery(this HttpRequest req, string key, string missing = null)
        {
            string s = null;
            if (req.Form != null && req.Form.ContainsKey(key))
            {
                s = StringHelpers.TrimOrNull(req.Form[key]);
            }
            if (s == null && req.Query != null && req.Query.ContainsKey(key))
            {
                s = StringHelpers.TrimOrNull(req.Query[key]);
            }
            return s ?? missing;
        }

        public static string GetString(this IQueryCollection qc, string key, string missing = null)
        {
            string s = null;
            if (s == null && qc != null && qc.ContainsKey(key))
            {
                s = StringHelpers.TrimOrNull(qc[key]);
            }
            return s ?? missing;
        }

        public static bool GetBool(this IQueryCollection qc, string key, bool missing = false)
            => Parse.ParseBool(qc.GetString(key), missing);
    }
}
