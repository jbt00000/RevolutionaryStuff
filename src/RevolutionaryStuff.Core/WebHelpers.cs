using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Microsoft.Extensions.Primitives;
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

    extension(HttpStatusCode source)
    {
        public bool Is200Level()
                => source >= HttpStatusCode.OK && source < HttpStatusCode.Ambiguous;
    }

    public static string CreateBasicAuthorizationHeaderValueParameter(string username, string password)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

    public static AuthenticationHeaderValue CreateBasicAuthorizationHeaderValue(string username, string password)
        => new("Basic", CreateBasicAuthorizationHeaderValueParameter(username, password));

    extension(HttpRequestHeaders sources)
    {
        public HttpRequestHeaders AddBasicAuthorization(string username, string password)
        {
            sources.Authorization = CreateBasicAuthorizationHeaderValue(username, password);
            return sources;
        }
        public HttpRequestHeaders AddJwtAuthorization(string bearerToken)
        {
            sources.Authorization = new AuthenticationHeaderValue(JwtAuthenticationScheme, bearerToken);
            return sources;
        }
        public void AcceptJson()
                => sources.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
    }

    public const string JwtAuthenticationScheme = "Bearer";



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

    extension(Uri source)
    {
        public StringValues GetQueryStringParam(string parameterName, bool caseSensitive = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentException("Parameter name cannot be null or empty.", nameof(parameterName));

            var queryParameters = HttpUtility.ParseQueryString(source.Query);
            if (caseSensitive)
            {
                return new StringValues(queryParameters.Get(parameterName));
            }
            else
            {
                foreach (var key in queryParameters.AllKeys)
                {
                    if (string.Equals(key, parameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        return new StringValues(queryParameters.Get(key));
                    }
                }
            }

            return StringValues.Empty;
        }
        public Uri AppendParameters(IEnumerable<KeyValuePair<string, string>> nameVals) =>
                new(AppendParameters(source.ToString(), nameVals));
        public Uri AppendParameter(string paramName, object val)
        {
            var url = AppendParameter(source.ToString(), paramName, val);
            return new Uri(url, source.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }
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



    extension(HttpResponseHeaders sources)
    {
        public string GetValueOrDefault(string headerName, string missing = null)
        {
            if (sources.TryGetValues(headerName, out var vals))
            {
                var e = vals.GetEnumerator();
                return e.MoveNext() ? e.Current : missing;
            }
            return missing;
        }
    }


    extension(HttpHeaders sources)
    {
        public void Set(string name, string val)
        {
            if (sources.Contains(name))
            {
                sources.Remove(name);
            }
            sources.Add(name, val);
        }
    }
}
