using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;
public static partial class WebHelpers
{
    /// <summary>
    /// A class that defines constant strings for commonly used WebDAV methods and utility methods for working with them.
    /// </summary>
    public static class WebDAVMethods
    {
        /// <summary>
        /// Represents the 'PROPFIND' WebDAV method.
        /// <see cref="https://tools.ietf.org/html/rfc4918#section-9.1"/>
        /// </summary>
        public const string PropFind = "PROPFIND";

        /// <summary>
        /// Represents the 'PROPPATCH' WebDAV method.
        /// <see cref="https://tools.ietf.org/html/rfc4918#section-9.2"/>
        /// </summary>
        public const string PropPatch = "PROPPATCH";

        /// <summary>
        /// Represents the 'MKCOL' WebDAV method.
        /// <see cref="https://tools.ietf.org/html/rfc4918#section-9.3"/>
        /// </summary>
        public const string MkCol = "MKCOL";

        /// <summary>
        /// Represents the 'DELETE' WebDAV method.
        /// <see cref="https://tools.ietf.org/html/rfc4918#section-9.6"/>
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        /// Represents the 'COPY' WebDAV method.
        /// <see cref="https://tools.ietf.org/html/rfc4918#section-9.8"/>
        /// </summary>
        public const string Copy = "COPY";

        /// <summary>
        /// Represents the 'MOVE' WebDAV method.
        /// <see cref="https://tools.ietf.org/html/rfc4918#section-9.9"/>
        /// </summary>
        public const string Move = "MOVE";

        /// <summary>
        /// Represents the 'LOCK' WebDAV method.
        /// <see cref="https://tools.ietf.org/html/rfc4918#section-9.10"/>
        /// </summary>
        public const string Lock = "LOCK";

        /// <summary>
        /// Represents the 'UNLOCK' WebDAV method.
        /// <see cref="https://tools.ietf.org/html/rfc4918#section-9.11"/>
        /// </summary>
        public const string Unlock = "UNLOCK";

        private static readonly Regex VerbExpr = new(string.Format(
            "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
            PropFind, PropPatch, MkCol, Delete, Copy, Move, Lock, Unlock
            ), RegexOptions.Compiled);

        /// <summary>
        /// Checks if a given string is a standard WebDAV verb.
        /// </summary>
        /// <param name="s">The WebDAV method as a string.</param>
        /// <returns><c>true</c> if the string is a standard WebDAV verb; otherwise, <c>false</c>.</returns>
        public static bool IsStandardVerb(string s)
        {
            return s != null && VerbExpr.IsMatch(s);
        }
    }
}
