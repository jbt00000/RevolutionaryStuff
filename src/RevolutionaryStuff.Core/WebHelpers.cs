using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using RevolutionaryStuff.Core.Collections;

namespace RevolutionaryStuff.Core;
public static partial class WebHelpers
{
    public static class CommonSchemes
    {
        public const string File = "file";
        public const string Http = "http";
        public const string Https = "https";
        public const string Ftp = "ftp";
        public const string SFtp = "sftp";
        public const string Ftps = "ftps";

        public static bool IsSecure(string scheme)
            => scheme is Https or SFtp or Ftps;

        public static bool IsInsecure(string scheme)
            => !IsSecure(scheme);
    }

    public static string CreateBasicAuthorizationHeaderValueParameter(string username, string password)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

    public static AuthenticationHeaderValue CreateBasicAuthorizationHeaderValue(string username, string password)
        => new("Basic", CreateBasicAuthorizationHeaderValueParameter(username, password));

    public static HttpRequestHeaders AddBasicAuthorization(this HttpRequestHeaders h, string username, string password)
    {
        h.Authorization = CreateBasicAuthorizationHeaderValue(username, password);
        return h;
    }

    public const string JwtAuthenticationScheme = "Bearer";

    public static HttpRequestHeaders AddJwtAuthorization(this HttpRequestHeaders h, string bearerToken)
    {
        h.Authorization = new AuthenticationHeaderValue(JwtAuthenticationScheme, bearerToken);
        return h;
    }

    public static HttpContent CreateJsonContent(string json, Encoding encoding = null)
        => new StringContent(json, encoding ?? Encoding.UTF8, "application/json");

    public static HttpContent CreateHttpContent(IEnumerable<KeyValuePair<string, string>> datas)
        => CreateHttpContent(datas.UrlEncode());

    public static HttpContent CreateHttpContent(string postData, Encoding e = null)
    {
        var content = new StreamContent(StreamHelpers.Create(postData, e ?? StreamHelpers.UTF8EncodingWithoutPreamble));
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
        return content;
    }

    public static MultipleValueDictionary<string, string> ParseQueryParams(string queryString)
    {
        var m = new MultipleValueDictionary<string, string>(Comparers.CaseInsensitiveStringComparer);
        if (queryString.StartsWith("?"))
        {
            queryString = queryString[1..];
        }
        var args = queryString.Split('&');
        foreach (var s in args)
        {
            var i = s.IndexOf('=');
            var n = s;
            string v = null;
            if (i > -1)
            {
                n = s[..i];
                v = Uri.UnescapeDataString(s[(i + 1)..]);
            }
            n = Uri.UnescapeDataString(n);
            m.Add(n, v);
        }
        return m;
    }

    public static Uri AppendParameters(this Uri uri, IEnumerable<KeyValuePair<string, string>> nameVals) =>
        new(AppendParameters(uri.ToString(), nameVals));

    public static string AppendParameters(string url, IEnumerable<KeyValuePair<string, string>> nameVals)
    {
        if (nameVals != null)
        {
            foreach (var kvp in nameVals)
            {
                url = AppendParameter(url, kvp.Key, kvp.Value);
            }
        }
        return url;
    }

    public static string AppendParameter(string url, string paramName, object val, bool includeEmptyVals = false)
    {
        if (!url.Contains("?"))
        {
            url += "?";
        }
        var lastChar = url[^1];
        if (lastChar is not '?' and not '&')
        {
            url += "&";
        }
        url += Uri.EscapeDataString(paramName);
        var ov = StringHelpers.ToString(val);
        var full = !string.IsNullOrEmpty(ov);
        if (includeEmptyVals || full)
        {
            url += "=";
        }
        if (full)
        {
            url += Uri.EscapeDataString(ov);
        }
        return url;
    }

    public static Uri AppendParameter(this Uri u, string paramName, object val)
    {
        var url = AppendParameter(u.ToString(), paramName, val);
        return new Uri(url, u.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
    }

    public static string GetValueOrDefault(this HttpResponseHeaders headers, string headerName, string missing = null)
    {
        if (headers.TryGetValues(headerName, out var vals))
        {
            var e = vals.GetEnumerator();
            return e.MoveNext() ? e.Current : missing;
        }
        return missing;
    }
    public static void AcceptJson(this HttpRequestHeaders headers)
        => headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

    public static void Set(this HttpHeaders headers, string name, string val)
    {
        if (headers.Contains(name))
        {
            headers.Remove(name);
        }
        headers.Add(name, val);
    }
}
