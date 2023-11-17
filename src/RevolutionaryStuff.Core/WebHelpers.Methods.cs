using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;
public static partial class WebHelpers
{
    /// <summary>
    /// A class that defines constant strings for commonly used HTTP methods and utility methods for working with them.
    /// </summary>
    public static class Methods
    {
        /// <summary>
        /// Represents the 'GET' HTTP method.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/GET"/>
        /// </summary>
        public const string Get = "GET";

        /// <summary>
        /// Represents the 'HEAD' HTTP method.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/HEAD"/>
        /// </summary>
        public const string Head = "HEAD";

        /// <summary>
        /// Represents the 'DELETE' HTTP method.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/DELETE"/>
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        /// Represents the 'PATCH' HTTP method.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/PATCH"/>
        /// </summary>
        public const string Patch = "PATCH";

        /// <summary>
        /// Represents the 'POST' HTTP method.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/POST"/>
        /// </summary>
        public const string Post = "POST";

        /// <summary>
        /// Represents the 'PUT' HTTP method.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/PUT"/>
        /// </summary>
        public const string Put = "PUT";

        /// <summary>
        /// Represents the 'OPTIONS' HTTP method.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/OPTIONS"/>
        /// </summary>
        public const string Options = "OPTIONS";

        /// <summary>
        /// Represents the 'TRACE' HTTP method.
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/TRACE"/>
        /// </summary>
        public const string Trace = "TRACE";

        private static readonly Regex VerbExpr = new(string.Format(
            "{0}|{1}|{2}|{3}|{4}|{5}|{6}",
            Get, Head, Post, Put, Options, Patch, Trace
            ), RegexOptions.Compiled);

        public static void RequiresStandardVerb(string s)
        {
            if (!IsStandardVerb(s))
            {
                throw new ArgumentException($"The HTTP method '{s}' is not a standard HTTP verb.");
            }
        }

        /// <summary>
        /// Checks if a given string is a standard web verb.
        /// </summary>
        /// <param name="s">The HTTP method as a string.</param>
        /// <returns><c>true</c> if the string is a standard web verb; otherwise, <c>false</c>.</returns>
        public static bool IsWebVerb(string s)
            => IsStandardVerb(s);

        /// <summary>
        /// Checks if a given string is a standard HTTP verb.
        /// </summary>
        /// <param name="s">The HTTP method as a string.</param>
        /// <returns><c>true</c> if the string is a standard HTTP verb; otherwise, <c>false</c>.</returns>
        public static bool IsStandardVerb(string s)
            => s != null && VerbExpr.IsMatch(s);

        /// <summary>
        /// Checks if a given string is either 'GET' or 'HEAD'.
        /// </summary>
        /// <param name="s">The HTTP method as a string.</param>
        /// <returns><c>true</c> if the string is either 'GET' or 'HEAD'; otherwise, <c>false</c>.</returns>
        public static bool IsGetOrHead(string s)
            => s is Get or Head;

        /// <summary>
        /// Checks if a given string is either 'POST', 'PUT', or 'PATCH'.
        /// </summary>
        /// <param name="s">The HTTP method as a string.</param>
        /// <returns><c>true</c> if the string is either 'POST', 'PUT', or 'PATCH'; otherwise, <c>false</c>.</returns>
        public static bool IsPostOrPutOrPatch(string s)
            => s is Post or Put or Patch;
    }
}
