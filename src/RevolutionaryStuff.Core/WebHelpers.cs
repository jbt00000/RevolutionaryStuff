using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using RevolutionaryStuff.Core.Collections;

namespace RevolutionaryStuff.Core;
public static class WebHelpers
{
    public static class HeaderStrings
    {
        public const string AcceptEncoding = "Accept-Encoding";
        public const string AcceptRanges = "Accept-Ranges";
        public const string AcceptTypes = "Accept";
        public const string AltLocation = "X-Gnutella-Alternate-Location";
        public const string Authorization = "Authorization";
        public const string AvailableRanges = "X-Available-Ranges";
        public const string CacheControl = "Cache-Control";
        public const string Connection = "Connection";
        public const string ContentDisposition = "Content-Disposition";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentLength = "Content-Length";
        public const string ContentRange = "Content-Range"; //used when responding
        public const string ContentType = "Content-Type";
        public const string ContentUrn = "X-Content-URN";
        public const string Cookie = "Cookie";
        public const string Date = "Date";
        public const string ETag = "ETag";
        public const string GnutellaAltLocation = "X-Alt";
        public const string GnutellaNegativeAltLocation = "X-NAlt";
        public const string Host = "Host";
        public const string IfModifiedSince = "If-Modified-Since";
        public const string IfNoneMatch = "If-None-Match";
        public const string IfRange = "If-Range";
        public const string IfUnmodifiedSince = "If-Unmodified-Since";
        public const string KeepAlive = "Keep-Alive";
        public const string LastModified = "Last-Modified";
        public const string Location = "Location";
        public const string MaxSlots = "X-Gnutella-maxSlots";
        public const string MaxSlotsPerHost = "X-Gnutella-maxSlotsPerHost";
        public const string ProxyConnection = "Proxy-Connection";
        public const string Range = "Range"; //used when requesting
        public const string Referer = "Referer";
        public const string ServentID = "X-Gnutella-Servent-ID";
        public const string Server = "Server";
        public const string SetCookie = "Set-Cookie";
        public const string UnlessModifiedSince = "Unless-Modified-Since";
        public const string UserAgent = "User-Agent";
        public const string WWWAuthenticate = "WWW-Authenticate";
    }

    public static class CommonSchemes
    {
        public const string File = "file";
        public const string Http = "http";
        public const string Https = "https";
        public const string Ftp = "ftp";
        public const string SFtp = "sftp";
        public const string Ftps = "ftps";

        public static bool IsSecure(string scheme)
            => scheme == Https || scheme == SFtp || scheme == Ftps;

        public static bool IsInsecure(string scheme)
            => !IsSecure(scheme);
    }

    public static class Methods
    {
        public const string Get = "GET";
        public const string Head = "HEAD";
        public const string Delete = "DELETE";
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Options = "OPTIONS";

        private static readonly Regex VerbExpr = new Regex(string.Format(
            "{0}|{1}|{2}|{3}|{4}",
            Get, Head, Post, Put, Options
            ), RegexOptions.Compiled);

        public static bool IsWebVerb(string s)
        {
            return IsStandardVerb(s);
        }

        public static bool IsStandardVerb(string s)
        {
            if (s == null) return false;
            return VerbExpr.IsMatch(s);
        }

        public static bool IsGetOrHead(string s)
        {
            return s == Get || s == Head;
        }
    }

    public static class WebDAVVerbs
    {
        public const string PropFind = "PROPFIND";
        public const string PropPatch = "PROPPATCH";
        public const string MkCol = "MKCOL";
        public const string Delete = "DELETE";
        public const string Copy = "COPY";
        public const string Move = "MOVE";
        public const string Lock = "LOCK";
        public const string Unlock = "UNLOCK";

        private static readonly Regex VerbExpr = new Regex(string.Format(
            "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
            PropFind, PropPatch, MkCol, Delete, Copy, Move, Lock, Unlock
            ), RegexOptions.Compiled);

        public static bool IsStandardVerb(string s)
        {
            if (s == null) return false;
            return VerbExpr.IsMatch(s);
        }
    }

    public static string CreateBasicAuthorizationHeaderValueParameter(string username, string password)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

    public static AuthenticationHeaderValue CreateBasicAuthorizationHeaderValue(string username, string password)
        => new AuthenticationHeaderValue("Basic", CreateBasicAuthorizationHeaderValueParameter(username, password));

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

    public static HttpContent CreateHttpContent(string postData, Encoding e=null)
    {
        var content = new StreamContent(StreamHelpers.Create(postData, e??StreamHelpers.UTF8EncodingWithoutPreamble));
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
        return content;
    }

    public static MultipleValueDictionary<string, string> ParseQueryParams(string queryString)
    {
        var m = new MultipleValueDictionary<string, string>(Comparers.CaseInsensitiveStringComparer);
        if (queryString.StartsWith("?"))
        {
            queryString = queryString.Substring(1);
        }
        string[] args = queryString.Split('&');
        foreach (string s in args)
        {
            int i = s.IndexOf('=');
            string n = s;
            string v = null;
            if (i > -1)
            {
                n = s.Substring(0, i);
                v = Uri.UnescapeDataString(s.Substring(i + 1));
            }
            n = Uri.UnescapeDataString(n);
            m.Add(n, v);
        }
        return m;
    }

    public static Uri AppendParameters(this Uri uri, IEnumerable<KeyValuePair<string, string>> nameVals)
        => new Uri(AppendParameters(uri.ToString(), nameVals));

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
        char lastChar = url[url.Length - 1];
        if (lastChar != '?' && lastChar != '&')
        {
            url += "&";
        }
        url += Uri.EscapeDataString(paramName);
        string ov = StringHelpers.ToString(val);
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
        IEnumerable<string> vals;
        if (headers.TryGetValues(headerName, out vals))
        {
            var e = vals.GetEnumerator();
            return e.MoveNext() ? e.Current : missing;
        }
        return missing;
    }
    public static void AcceptJson(this HttpRequestHeaders headers)
        => headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
}
