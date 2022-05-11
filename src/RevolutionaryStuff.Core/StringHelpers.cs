using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

public static class StringHelpers
{
    public static string DecodeBase64String(this string base64)
    {
        base64 = TrimOrNull(base64);
        if (base64 == null) return null;
        var buf = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(buf);
    }

    public static string ToBase64String(this string s)
    {
        if (s == null) return null;
        var buf = Encoding.UTF8.GetBytes(s);
        return Convert.ToBase64String(buf);
    }

    public static string AppendWithConditionalAppendPrefix(this string baseString, string conditionalAppendPrefix, string baseAppend)
    {
        baseString ??= "";
        if (baseString.Length > 0 && baseAppend != null && baseAppend.Length > 0)
        {
            baseString += conditionalAppendPrefix ?? "";
        }
        baseString += baseAppend ?? "";
        return baseString;
    }

    private static bool ContainsLessThanEq(this string s, char chMax)
    {
        if (s == null) return true;
        foreach (var ch in s)
        {
            if (ch > chMax) return false;
        }
        return true;
    }

    /// <summary>
    /// Is this a 7bit string
    /// </summary>
    /// <param name="s">The string to test</param>
    /// <returns>True if all characters fit in 7 bits, else false</returns>
    public static bool ContainsOnlyAsciiCharacters(this string s)
        => s.ContainsLessThanEq((char)127);

    /// <summary>
    /// Is this a 8bit string
    /// </summary>
    /// <param name="s">The string to test</param>
    /// <returns>True if all characters fit in 8 bits, else false</returns>
    public static bool ContainsOnlyExtendedAsciiCharacters(this string s)
        => s.ContainsLessThanEq((char)255);

    public static string Replace(this string s, Match m, string replacement = null)
    {
        if (m.Success)
        {
            s = s[..m.Index] + (replacement ?? "") + s[(m.Index + m.Length)..];
        }
        return s;
    }

    public static string ToTitleFriendlyString(this string s)
    {
        var sb = new StringBuilder();
        var lastWasUpper = false;
        var lastWasUnderscore = false;
        for (var x = 0; x < s.Length; ++x)
        {
            var ch = s[x];
            if (char.IsUpper(ch))
            {
                if (!lastWasUpper && x > 0)
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
                if (lastWasUnderscore || x == 0)
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

    public static string ToLowerCamelCase(this string s)
    {
        s = s.ToUpperCamelCase();
        if (s.Length > 0 && char.IsUpper(s[0]))
        {
            s = s[0].ToString().ToLower() + s[1..];
        }
        return s;
    }

    public static string ToTitleCase(this string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var lastWasLetter = false;
        var sb = new StringBuilder(s.Length);
        for (var z = 0; z < s.Length; ++z)
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

    public static string Right(this string s, int lastNChars)
    {
        if (s == null || s.Length <= lastNChars) return s;
        return s[^lastNChars..];
    }

    public static string Left(this string s, int firstNChars)
    {
        if (s == null) return null;
        if (s.Length > firstNChars)
        {
            s = s[..firstNChars];
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
        var n = first ? s.IndexOf(sep) : s.LastIndexOf(sep);
        left = right = "";
        if (n < 0)
        {
            left = s;
            return false;
        }
        else
        {
            left = s[..n];
            right = s[(n + sep.Length)..];
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

    public static string RightOf(this string s, string pivot, bool returnFullStringIfPivotIsMissing = false)
    {
        if (s == null) return null;
        string left, right;
        if (s.Split(pivot, true, out left, out right)) return right;
        return returnFullStringIfPivotIsMissing ? s : null;
    }

    public static bool Contains(this string s, string value, bool ignoreCase = false)
    {
        if (s == null) return false;
        if (!ignoreCase) return s.Contains(value);
        return s.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) > -1;
    }

    public static string TruncateWithEllipsis(this string s, int len, string ellipsis = "...")
    {
        if (s == null) return null;
        if (s.Length > len)
        {
            s = s[..(len - ellipsis.Length)] + ellipsis;
            Debug.Assert(s.Length == len);
        }
        return s;
    }

    public static string TruncateWithMidlineEllipsis(this string s, int len, string ellipsis = "---")
    {
        if (s == null) return null;
        if (s.Length > len)
        {
            var pivot = len * 2 / 3;
            var left = s[..(pivot - ellipsis.Length)];
            var right = s[^(len - pivot)..];
            s = left + ellipsis + right;
            Debug.Assert(s.Length == len);
        }
        return s;
    }

    public static bool IsSameIgnoreCase(string a, string b)
        => 0 == CompareIgnoreCase(a, b);

    public static int CompareIgnoreCase(string a, string b)
        => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    /// Returns the first non-empty string.  Or null if there are no such items
    /// </summary>
    /// <param name="vals"></param>
    /// <returns>The first non-empty string.  Or null if there are no such items.</returns>
    public static string Coalesce(params string[] vals)
    {
        if (vals != null)
        {
            for (var x = 0; x < vals.Length; ++x)
            {
                var s = vals[x];
                if (string.IsNullOrWhiteSpace(s)) continue;
                return s;
            }
        }
        return null;
    }

    public static string ToString(this object o, string nullValue = null)
        => o?.ToString() ?? nullValue;

    public static string[] Split(this string s, Regex r)
    {
        Requires.NonNull(r, nameof(r));
        return r.Split(s);
    }

    public static string RemoveSpecialCharacters(this string str)
    {
        var sb = new StringBuilder();
        foreach (var c in str)
        {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Remove the funky ass Diacritic marks (accepts) from a string
    /// </summary>
    /// <param name="s">The input string which may contain accent marks</param>
    /// <returns>The cleanup up output string</returns>
    /// <remarks>http://blogs.msdn.com/michkap/archive/2005/02/19/376617.aspx</remarks>
    /// <remarks>https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net</remarks>
    public static string RemoveDiacritics(this string s)
    {
        var stFormD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        for (var ich = 0; ich < stFormD.Length; ich++)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(stFormD[ich]);
            }
        }

        return (sb.ToString());
    }

    private static readonly Regex NameArgExpr = new("(?<!{){\\s*(?'term'\\w+)(?'modifiers'|[:,][^}]+)}", RegexOptions.Compiled);

    public static string FormatWithNamedArgs(string format, string k0, object v0, object missingVal = null)
        => FormatWithNamedArgs(format, new[] { new KeyValuePair<string, object>(k0, v0) }, missingVal);

    public static string FormatWithNamedArgs(string format, IEnumerable<KeyValuePair<string, object>> args, object missingVal = null)
    {
        Requires.NonNull(format, nameof(format));

        var d = args.NullSafeEnumerable().ToDictionary(z => z.Key, z => z.Value);

        return NameArgExpr.Replace(format, me => string.Format("{0" + me.Groups["modifiers"].Value + "}", d.GetValue(me.Groups["term"].Value, missingVal)));
    }
}
