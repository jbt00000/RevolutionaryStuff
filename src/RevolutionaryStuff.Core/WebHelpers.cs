using RevolutionaryStuff.Core.Collections;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core
{
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

        public static HttpContent CreateHttpContent(IEnumerable<KeyValuePair<string, string>> datas)
        {
            return CreateHttpContent(datas.UrlEncode());
        }

        public static HttpContent CreateHttpContent(string postData)
        {
            var content = new StreamContent(StreamHelpers.Create(postData));
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

        public static Uri AppendParameters(Uri uri, IEnumerable<KeyValuePair<string, string>> nameVals)
        {
            return new Uri(AppendParameters(uri.ToString(), nameVals));
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
#if false
        public static string DownloadBodyToText(Uri uri)
        {
            Requires.NonNull(uri, nameof(uri));
            var req = WebRequest.Create(uri);
            using (var resp = req.GetResponse())
            {
                using (var st = resp.GetResponseStream())
                {
                    return st.ReadToEnd();
                }
            }
        }
#endif

        public static string GetValueOrDefault(this HttpResponseHeaders headers, string headerName, string missing=null)
        {
            IEnumerable<string> vals;
            if (headers.TryGetValues(headerName, out vals))
            {
                var e = vals.GetEnumerator();
                return e.MoveNext() ? e.Current : missing;
            }
            return missing;
        }
    }
}
