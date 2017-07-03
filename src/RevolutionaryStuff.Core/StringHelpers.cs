using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core
{
    public static class StringHelpers
    {
        public static string Replace(this string s, Match m, string replacement=null)
        {
            if (m.Success)
            {
                s = s.Substring(0, m.Index) + (replacement??"") + s.Substring(m.Index + m.Length);
            }
            return s;
        }

        public static string ToTitleFriendlyString(this string s)
        {
            var sb = new StringBuilder();
            bool lastWasUpper = false;
            bool lastWasUnderscore = false;
            for (int x = 0; x < s.Length; ++x)
            {
                var ch = s[x];
                if (char.IsUpper(ch))
                {
                    if (!lastWasUpper && x>0)
                    {
                        sb.Append(' ');
                    }
                    lastWasUpper = true;
                    lastWasUnderscore = false;
                }
                else if (ch == '_')
                {
                    ch = ' ';
                    lastWasUnderscore = true;
                }
                else
                {
                    if (lastWasUnderscore || x==0)
                    {
                        ch = char.ToUpper(ch);
                    }
                    lastWasUpper = false;
                    lastWasUnderscore = false;
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        public static string ToUpperCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.ToLower().ToTitleCase();
            s = RegexHelpers.Common.Whitespace.Replace(s, "");
            return s;
        }

        public static string ToTitleCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            bool lastWasLetter = false;
            var sb = new StringBuilder(s.Length);
            for (int z = 0; z < s.Length; ++z)
            {
                var ch = s[z];
                if (char.IsLetter(ch))
                {
                    ch = lastWasLetter ? char.ToLower(ch) : char.ToUpper(ch);
                    lastWasLetter = true;
                }
                else
                {
                    lastWasLetter = false;
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        public static string Left(this string s, int firstNChars)
        {
            if (s == null) return null;
            if (s.Length > firstNChars)
            {
                s = s.Substring(0, firstNChars);
            }
            return s;
        }

        public static string TrimOrNull(this string s, int? maxLength = null)
        {
            if (s != null)
            {
                s = s.Trim();
                if (s.Length == 0) s = null;
            }
            if (s != null && maxLength.HasValue)
            {
                s = s.Left(maxLength.Value);
            }
            return s;
        }

        /// <summary>
        /// Splits a string on a pivot point
        /// </summary>
        /// <param name="s">The string</param>
        /// <param name="sep">The pivot, this will not be included</param>
        /// <param name="first">When true, use the first instance of the pivot, else use the last</param>
        /// <param name="left">The left side of the pivot point</param>
        /// <param name="right">The right side of the pivot point</param>
        /// <returns>true if a split occurred, else false</returns>
        /// <example>
        /// StringSides("1234567", "34", true, "12", "567")
        /// </example>
        public static bool Split(this string s, string sep, bool first, out string left, out string right)
        {
            int n = first ? s.IndexOf(sep) : s.LastIndexOf(sep);
            left = right = "";
            if (n < 0)
            {
                left = s;
                return false;
            }
            else
            {
                left = s.Substring(0, n);
                right = s.Substring(n + sep.Length);
                return true;
            }
        }

        public static string LeftOf(this string s, string pivot)
        {
            if (s == null) return null;
            string left, right;
            s.Split(pivot, true, out left, out right);
            return left;
        }

        public static string RightOf(this string s, string pivot, bool returnFullStringIfPivotIsMissing=false)
        {
            if (s == null) return null;
            string left, right;
            return s.Split(pivot, true, out left, out right) || !returnFullStringIfPivotIsMissing ? right : s;
        }

        public static bool Contains(this string s, string value, bool ignoreCase=false)
        {
            if (s == null) return false;
            if (!ignoreCase) return s.Contains(value);
            return s.IndexOf(value, System.StringComparison.CurrentCultureIgnoreCase) > -1;
        }

        public static string TruncateWithEllipsis(this string s, int len, string ellipsis = "...")
        {
            if (s == null) return null;
            if (s.Length >= len)
            {
                s = s.Substring(0, len - ellipsis.Length) + ellipsis;
                Debug.Assert(s.Length == len);
            }
            return s;
        }

        public static string TruncateWithMidlineEllipsis(this string s, int len, string ellipsis = "---")
        {
            if (s == null) return null;
            if (s.Length >= len)
            {
                int pivot = len * 2 / 3;
                var left = s.Substring(0, pivot - ellipsis.Length);
                var right = s.Substring(s.Length - (len - pivot));
                s = left + ellipsis + right;
                Debug.Assert(s.Length == len);
            }
            return s;
        }

        public static bool IsSameIgnoreCase(string a, string b)
        {
            return 0 == CompareIgnoreCase(a, b);
        }

        public static int CompareIgnoreCase(string a, string b)
        {
            return string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Returns the first non-empty string.  Or null if there are no such items
        /// </summary>
        /// <param name="vals"></param>
        /// <returns>The first non-empty string.  Or null if there are no such items.</returns>
        public static string Coalesce(params string[] vals)
        {
            if (vals != null)
            {
                for (int x = 0; x < vals.Length; ++x)
                {
                    if (vals[x] != null && vals[x].Length > 0) return vals[x];
                }
            }
            return null;
        }

        public static string ToString(this object o, string nullValue=null)
            => o?.ToString() ?? nullValue;

        public static string[] Split(this string s, Regex r)
        {
            Requires.NonNull(r, nameof(r));
            return r.Split(s);
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string ToCrmHeading(this string str)
        {
            str = str.ToTitleFriendlyString();
            str = str.Replace("Contact", "");
            str = str.Replace("List", "");
            str = str.Replace("Listing", "");
            return str;
        }

        public static string PrependQuestionMark(this string str)
        {
            str = "?" + str;
            return str;
        }
    }
}
