namespace RevolutionaryStuff.Core;

public static partial class WebHelpers
{
    /// <summary>
    /// A class that defines constant strings for commonly used HTTP headers.
    /// </summary>
    public static class HeaderStrings
    {
        /// <summary>
        /// Represents the 'Accept-Encoding' HTTP header. 
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Encoding"/>
        /// </summary>
        public const string AcceptEncoding = "Accept-Encoding";

        /// <summary>
        /// Represents the 'Accept-Ranges' HTTP header. 
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Ranges"/>
        /// </summary>
        public const string AcceptRanges = "Accept-Ranges";

        /// <summary>
        /// Represents the 'Accept' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept"/>
        /// </summary>
        public const string AcceptTypes = "Accept";

        /// <summary>
        /// Represents the 'Authorization' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization"/>
        /// </summary>
        public const string Authorization = "Authorization";

        // Custom header, no official documentation link available
        /// <summary>
        /// Represents the 'X-Available-Ranges' custom header.
        /// </summary>
        public const string AvailableRanges = "X-Available-Ranges";

        /// <summary>
        /// Represents the 'Cache-Control' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control"/>
        /// </summary>
        public const string CacheControl = "Cache-Control";

        /// <summary>
        /// Represents the 'Connection' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Connection"/>
        /// </summary>
        public const string Connection = "Connection";

        /// <summary>
        /// Represents the 'Content-Disposition' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Disposition"/>
        /// </summary>
        public const string ContentDisposition = "Content-Disposition";

        /// <summary>
        /// Represents the 'Content-Encoding' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Encoding"/>
        /// </summary>
        public const string ContentEncoding = "Content-Encoding";

        /// <summary>
        /// Represents the 'Content-Length' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Length"/>
        /// </summary>
        public const string ContentLength = "Content-Length";

        /// <summary>
        /// Represents the 'Content-Range' HTTP header used when responding.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Range"/>
        /// </summary>
        public const string ContentRange = "Content-Range";

        /// <summary>
        /// Represents the 'Content-Type' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Type"/>
        /// </summary>
        public const string ContentType = "Content-Type";

        // Custom header, no official documentation link available
        /// <summary>
        /// Represents the 'X-Content-URN' custom header.
        /// </summary>
        public const string ContentUrn = "X-Content-URN";

        /// <summary>
        /// Represents the 'Cookie' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cookie"/>
        /// </summary>
        public const string Cookie = "Cookie";

        /// <summary>
        /// Represents the 'Date' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Date"/>
        /// </summary>
        public const string Date = "Date";

        /// <summary>
        /// Represents the 'ETag' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag"/>
        /// </summary>
        public const string ETag = "ETag";

        /// <summary>
        /// Represents the 'Host' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Host"/>
        /// </summary>
        public const string Host = "Host";

        /// <summary>
        /// Represents the 'If-Modified-Since' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/If-Modified-Since"/>
        /// </summary>
        public const string IfModifiedSince = "If-Modified-Since";

        /// <summary>
        /// Represents the 'If-None-Match' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/If-None-Match"/>
        /// </summary>
        public const string IfNoneMatch = "If-None-Match";

        /// <summary>
        /// Represents the 'If-Range' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/If-Range"/>
        /// </summary>
        public const string IfRange = "If-Range";

        /// <summary>
        /// Represents the 'If-Unmodified-Since' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/If-Unmodified-Since"/>
        /// </summary>
        public const string IfUnmodifiedSince = "If-Unmodified-Since";

        /// <summary>
        /// Represents the 'Keep-Alive' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Keep-Alive"/>
        /// </summary>
        public const string KeepAlive = "Keep-Alive";

        /// <summary>
        /// Represents the 'Last-Modified' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Last-Modified"/>
        /// </summary>
        public const string LastModified = "Last-Modified";

        /// <summary>
        /// Represents the 'Location' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Location"/>
        /// </summary>
        public const string Location = "Location";

        /// <summary>
        /// Represents the 'Proxy-Connection' HTTP header.
        /// </summary>
        public const string ProxyConnection = "Proxy-Connection";

        /// <summary>
        /// Represents the 'Range' HTTP header used when requesting.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Range"/>
        /// </summary>
        public const string Range = "Range";

        /// <summary>
        /// Represents the 'Referer' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referer"/>
        /// </summary>
        public const string Referer = "Referer";

        /// <summary>
        /// Represents the 'Server' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Server"/>
        /// </summary>
        public const string Server = "Server";

        /// <summary>
        /// Represents the 'Set-Cookie' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie"/>
        /// </summary>
        public const string SetCookie = "Set-Cookie";

        // Custom header, no official documentation link available
        /// <summary>
        /// Represents the 'Unless-Modified-Since' custom header.
        /// </summary>
        public const string UnlessModifiedSince = "Unless-Modified-Since";

        /// <summary>
        /// Represents the 'User-Agent' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent"/>
        /// </summary>
        public const string UserAgent = "User-Agent";

        /// <summary>
        /// Represents the 'WWW-Authenticate' HTTP header.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/WWW-Authenticate"/>
        /// </summary>
        public const string WWWAuthenticate = "WWW-Authenticate";
    }
}
