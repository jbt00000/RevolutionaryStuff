using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides utility methods for string manipulation, formatting, encoding, and transformation operations.
/// </summary>
public static partial class StringHelpers
{
    /// <summary>
    /// Condenses multiple consecutive whitespace characters into a single space.
    /// </summary>
    /// <param name="s">The string to process.</param>
    /// <returns>The string with condensed whitespace, or <c>null</c> if the input is <c>null</c>.</returns>
    public static string CondenseWhitespace(string s)
        => s == null ? null : RegexHelpers.Common.MultipleWhitespace().Replace(s, " ");

    /// <summary>
    /// Decodes a Base64-encoded string to its original UTF-8 string representation.
    /// </summary>
    /// <param name="base64">The Base64-encoded string to decode.</param>
    /// <returns>The decoded UTF-8 string, or <c>null</c> if the input is <c>null</c> or empty after trimming.</returns>
    public static string DecodeBase64String(this string base64)
    {
        base64 = TrimOrNull(base64);
        if (base64 == null) return null;
        var buf = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(buf);
    }

    /// <summary>
    /// Encodes a string to its Base64 representation using UTF-8 encoding.
    /// </summary>
    /// <param name="s">The string to encode.</param>
    /// <returns>The Base64-encoded string, or <c>null</c> if the input is <c>null</c>.</returns>
    public static string ToBase64String(this string s)
    {
        if (s == null) return null;
        var buf = Encoding.UTF8.GetBytes(s);
        return Convert.ToBase64String(buf);
    }

    /// <summary>
    /// Appends a formatted string to the base string if the value is not null.
    /// </summary>
    /// <param name="baseString">The base string to append to.</param>
    /// <param name="format">The format string. Should contain a {0} placeholder.</param>
    /// <param name="val">The value to format and append. If null, no operation is performed.</param>
    /// <returns>The updated string with the formatted value appended, or the original base string if val is null.</returns>
    public static string AppendFormatIfValNotNull(this string baseString, string format, object val)
    {
        if (val != null)
        {
            if (baseString == null)
            {
                baseString = string.Format(format, val);
            }
            else
            {
                baseString += string.Format(format, val);
            }
        }
        return baseString;
    }

    /// <summary>
    /// Appends a string to the base string, optionally adding a prefix if the base string already has content.
    /// </summary>
    /// <param name="baseString">The base string to append to.</param>
    /// <param name="conditionalAppendPrefix">The prefix to add before baseAppend if baseString is not empty (e.g., ", " or "; ").</param>
    /// <param name="baseAppend">The string to append.</param>
    /// <returns>The concatenated result.</returns>
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
    /// Determines whether the string contains only 7-bit ASCII characters (0-127).
    /// </summary>
    /// <param name="s">The string to test.</param>
    /// <returns><c>true</c> if all characters are 7-bit ASCII; otherwise, <c>false</c>. Returns <c>true</c> for null strings.</returns>
    public static bool ContainsOnlyAsciiCharacters(this string s)
        => s.ContainsLessThanEq((char)127);

    /// <summary>
    /// Determines whether the string contains only 8-bit extended ASCII characters (0-255).
    /// </summary>
    /// <param name="s">The string to test.</param>
    /// <returns><c>true</c> if all characters fit in 8 bits; otherwise, <c>false</c>. Returns <c>true</c> for null strings.</returns>
    public static bool ContainsOnlyExtendedAsciiCharacters(this string s)
        => s.ContainsLessThanEq((char)255);

    /// <summary>
    /// Replaces the matched portion of a string with a replacement string.
    /// </summary>
    /// <param name="s">The source string.</param>
    /// <param name="m">The regex match indicating what to replace.</param>
    /// <param name="replacement">The replacement string. Defaults to empty string if null.</param>
    /// <returns>The string with the match replaced, or the original string if the match was not successful.</returns>
    public static string Replace(this string s, Match m, string replacement = null)
    {
        if (m.Success)
        {
            s = s[..m.Index] + (replacement ?? "") + s[(m.Index + m.Length)..];
        }
        return s;
    }

    /// <summary>
    /// Converts a string to a title-friendly format by adding spaces before capital letters and after underscores.
    /// </summary>
    /// <param name="s">The string to convert (e.g., "MyPropertyName" or "my_property_name").</param>
    /// <returns>A title-friendly string (e.g., "My Property Name").</returns>
    /// <example>
    /// "MyPropertyName".ToTitleFriendlyString() returns "My Property Name"
    /// "my_property_name".ToTitleFriendlyString() returns "My Property Name"
    /// </example>
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

    /// <summary>
    /// Converts a string to UpperCamelCase (PascalCase) by removing spaces and capitalizing the first letter of each word.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>The string in UpperCamelCase format (e.g., "HelloWorld").</returns>
    /// <example>
    /// "hello world".ToUpperCamelCase() returns "HelloWorld"
    /// </example>
    public static string ToUpperCamelCase(this string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.ToLower().ToTitleCase();
        s = RegexHelpers.Common.Whitespace().Replace(s, "");
        return s;
    }

    /// <summary>
    /// Converts a string to lowerCamelCase by removing spaces and making the first letter lowercase.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>The string in lowerCamelCase format (e.g., "helloWorld").</returns>
    /// <example>
    /// "hello world".ToLowerCamelCase() returns "helloWorld"
    /// </example>
    public static string ToLowerCamelCase(this string s)
    {
        s = s.ToUpperCamelCase();
        if (s.Length > 0 && char.IsUpper(s[0]))
        {
            s = s[0].ToString().ToLower() + s[1..];
        }
        return s;
    }

    /// <summary>
    /// Converts a string to Title Case where the first letter of each word is capitalized.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>The string in Title Case format (e.g., "Hello World").</returns>
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

    /// <summary>
    /// Returns the rightmost characters of a string.
    /// </summary>
    /// <param name="s">The source string.</param>
    /// <param name="lastNChars">The number of characters to return from the end.</param>
    /// <returns>The last N characters, or the entire string if it's shorter than N characters.</returns>
    public static string Right(this string s, int lastNChars)
    {
        return s == null || s.Length <= lastNChars ? s : s[^lastNChars..];
    }

    /// <summary>
    /// Returns the leftmost characters of a string.
    /// </summary>
    /// <param name="s">The source string.</param>
    /// <param name="firstNChars">The number of characters to return from the beginning.</param>
    /// <returns>The first N characters, or the entire string if it's shorter than N characters. Returns null if input is null.</returns>
    public static string Left(this string s, int firstNChars)
    {
        if (s == null) return null;
        if (s.Length > firstNChars)
        {
            s = s[..firstNChars];
        }
        return s;
    }

    /// <summary>
    /// Determines whether a string is not null and has at least one character.
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <returns><c>true</c> if the string is not null and not empty; otherwise, <c>false</c>.</returns>
    public static bool NullSafeHasData(this string s)
        => s != null && s.Length > 0;

    /// <summary>
    /// Trims whitespace from a string and returns null if the result is empty.
    /// Optionally truncates to a maximum length.
    /// </summary>
    /// <param name="s">The string to trim.</param>
    /// <param name="maxLength">Optional maximum length to truncate to after trimming.</param>
    /// <returns>The trimmed string, or null if the input is null or becomes empty after trimming.</returns>
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
    /// Splits a string at the first or last occurrence of a separator.
    /// </summary>
    /// <param name="s">The string to split.</param>
    /// <param name="sep">The separator string.</param>
    /// <param name="first">If <c>true</c>, splits at the first occurrence; otherwise, splits at the last occurrence.</param>
    /// <param name="left">The substring before the separator.</param>
    /// <param name="right">The substring after the separator.</param>
    /// <returns><c>true</c> if the separator was found and a split occurred; otherwise, <c>false</c>.</returns>
    /// <example>
    /// "1234567".Split("34", true, out var left, out var right) returns left="12", right="567"
    /// </example>
    public static bool Split(this string s, string sep, bool first, out string left, out string right)
    {
        var n = first ? s.IndexOf(sep) : s.LastIndexOf(sep);
        _ = right = "";
        if (n < 0)
        {
            left = s;
            return false;
        }

        left = s[..n];
        right = s[(n + sep.Length)..];
        return true;
    }

    /// <summary>
    /// Returns the portion of the string before the first occurrence of a pivot string.
    /// </summary>
    /// <param name="s">The source string.</param>
    /// <param name="pivot">The pivot string to search for.</param>
    /// <returns>The substring before the pivot, or the entire string if the pivot is not found. Returns null if input is null.</returns>
    public static string LeftOf(this string s, string pivot)
    {
        if (s == null) return null;
        s.Split(pivot, true, out var left, out _);
        return left;
    }

    /// <summary>
    /// Returns the portion of the string after the first occurrence of a pivot string.
    /// </summary>
    /// <param name="s">The source string.</param>
    /// <param name="pivot">The pivot string to search for.</param>
    /// <param name="returnFullStringIfPivotIsMissing">
    /// If <c>true</c>, returns the full string when pivot is not found; 
    /// if <c>false</c>, returns null when pivot is not found.
    /// </param>
    /// <returns>The substring after the pivot, or null/full string depending on parameters. Returns null if input is null.</returns>
    public static string RightOf(this string s, string pivot, bool returnFullStringIfPivotIsMissing = false)
    {
        return s == null ? null : s.Split(pivot, true, out _, out var right) ? right : returnFullStringIfPivotIsMissing ? s : null;
    }

    /// <summary>
    /// Determines whether a string contains a specified substring, with optional case-insensitive comparison.
    /// </summary>
    /// <param name="s">The string to search in.</param>
    /// <param name="value">The substring to search for.</param>
    /// <param name="ignoreCase">If <c>true</c>, performs case-insensitive comparison; otherwise, case-sensitive.</param>
    /// <returns><c>true</c> if the substring is found; otherwise, <c>false</c>. Returns <c>false</c> if input is null.</returns>
    public static bool Contains(this string s, string value, bool ignoreCase = false)
    {
        return s != null && (!ignoreCase ? s.Contains(value) : s.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) > -1);
    }

    /// <summary>
    /// Truncates a string to a specified length and appends an ellipsis if truncated.
    /// </summary>
    /// <param name="s">The string to truncate.</param>
    /// <param name="len">The maximum length including the ellipsis.</param>
    /// <param name="ellipsis">The ellipsis string to append. Defaults to "...".</param>
    /// <returns>The truncated string with ellipsis, or the original string if shorter than len. Returns null if input is null.</returns>
    /// <example>
    /// "Hello World".TruncateWithEllipsis(8) returns "Hello..."
    /// </example>
    public static string TruncateWithEllipsis(this string s, int len, string ellipsis = "...")
    {
        if (s == null) return null;
        if (len <= 0) return string.Empty;
        if (s.Length > len)
        {
            if (ellipsis.Length >= len)
            {
                return ellipsis[..len]; // Return truncated ellipsis if it's longer than allowed length
            }
            s = s[..(len - ellipsis.Length)] + ellipsis;
            Debug.Assert(s.Length == len);
        }
        return s;
    }

    /// <summary>
    /// Truncates a string by placing an ellipsis in the middle, preserving the beginning and end.
    /// </summary>
    /// <param name="s">The string to truncate.</param>
    /// <param name="len">The maximum length including the ellipsis.</param>
    /// <param name="ellipsis">The ellipsis string to insert. Defaults to "---".</param>
    /// <returns>
    /// The truncated string with ellipsis in the middle, or the original string if shorter than len.
    /// Returns null if input is null.
    /// </returns>
    /// <example>
    /// "HelloWorld".TruncateWithMidlineEllipsis(8, "...") returns "He...rld"
    /// </example>
    public static string TruncateWithMidlineEllipsis(this string s, int len, string ellipsis = "---")
    {
        if (s == null) return null;
        if (len <= 0) return string.Empty;
        if (s.Length > len)
        {
            if (ellipsis.Length >= len)
            {
                return ellipsis[..len]; // Return truncated ellipsis if it's longer than allowed length
            }

            var pivot = len * 2 / 3;
            var left = s[..(pivot - ellipsis.Length)];
            var right = s[^(len - pivot)..];
            s = left + ellipsis + right;
            Debug.Assert(s.Length == len);
        }
        return s;
    }

    /// <summary>
    /// Compares two strings for equality using case-insensitive comparison.
    /// </summary>
    /// <param name="a">The first string.</param>
    /// <param name="b">The second string.</param>
    /// <returns><c>true</c> if the strings are equal (ignoring case); otherwise, <c>false</c>.</returns>
    public static bool IsSameIgnoreCase(string a, string b)
        => 0 == CompareIgnoreCase(a, b);

    /// <summary>
    /// Compares two strings using case-insensitive comparison.
    /// </summary>
    /// <param name="a">The first string.</param>
    /// <param name="b">The second string.</param>
    /// <returns>
    /// A value less than 0 if a is less than b, 0 if they are equal, or greater than 0 if a is greater than b.
    /// </returns>
    public static int CompareIgnoreCase(string a, string b)
        => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    /// Returns the first non-empty, non-whitespace string from the provided values.
    /// </summary>
    /// <param name="vals">An array of strings to evaluate.</param>
    /// <returns>The first non-empty string, or null if all values are null, empty, or whitespace.</returns>
    public static string Coalesce(params string[] vals)
    {
        if (vals != null)
        {
            foreach (var s in vals)
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                return s;
            }
        }
        return null;
    }

    /// <summary>
    /// Converts an object to its string representation, with a fallback value for null objects.
    /// </summary>
    /// <param name="o">The object to convert.</param>
    /// <param name="nullValue">The value to return if the object is null. Defaults to null.</param>
    /// <returns>The string representation of the object, or the nullValue if the object is null.</returns>
    public static string ToString(this object o, string nullValue = null)
        => o?.ToString() ?? nullValue;

    /// <summary>
    /// Splits a string using a regular expression pattern.
    /// </summary>
    /// <param name="s">The string to split.</param>
    /// <param name="r">The regular expression to use as the delimiter.</param>
    /// <returns>An array of substrings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the regex parameter is null.</exception>
    public static string[] Split(this string s, Regex r)
    {
        ArgumentNullException.ThrowIfNull(r);
        return r.Split(s);
    }

    /// <summary>
    /// Removes all special characters from a string, keeping only alphanumeric characters (0-9, A-Z, a-z).
    /// </summary>
    /// <param name="str">The string to process.</param>
    /// <returns>A string containing only alphanumeric characters.</returns>
    public static string RemoveSpecialCharacters(this string str)
    {
        var sb = new StringBuilder();
        foreach (var c in str)
        {
            if (c is (>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z'))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Removes diacritic marks (accents) from a string, converting accented characters to their base forms.
    /// </summary>
    /// <param name="s">The input string which may contain accent marks.</param>
    /// <returns>The string with diacritic marks removed (e.g., "café" becomes "cafe").</returns>
    /// <remarks>
    /// Uses Unicode normalization (FormD) to separate base characters from combining marks.
    /// References: http://blogs.msdn.com/michkap/archive/2005/02/19/376617.aspx and
    /// https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net
    /// </remarks>
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

        return sb.ToString();
    }

    [GeneratedRegex("(?<!{){\\s*(?'term'\\w+)(?'modifiers'|[:,][^}]+)}")]
    private static partial Regex NameArgExpr { get; }

    /// <summary>
    /// Formats a string with a single named argument, replacing {name} placeholders with values.
    /// </summary>
    /// <param name="format">The format string containing {name} placeholders.</param>
    /// <param name="k0">The name of the argument.</param>
    /// <param name="v0">The value of the argument.</param>
    /// <param name="missingVal">The value to use for missing arguments. Defaults to null.</param>
    /// <returns>The formatted string with placeholders replaced.</returns>
    /// <exception cref="ArgumentNullException">Thrown when format is null.</exception>
    /// <example>
    /// FormatWithNamedArgs("Hello {name}!", "name", "World") returns "Hello World!"
    /// </example>
    public static string FormatWithNamedArgs(string format, string k0, object v0, object missingVal = null)
        => FormatWithNamedArgs(format, [new KeyValuePair<string, object>(k0, v0)], missingVal);

    /// <summary>
    /// Formats a string with named arguments, replacing {name} placeholders with values from a dictionary.
    /// Supports format modifiers like {name:N2} or {name,10}.
    /// </summary>
    /// <param name="format">The format string containing {name} placeholders.</param>
    /// <param name="args">A collection of key-value pairs providing argument names and values.</param>
    /// <param name="missingVal">The value to use for missing arguments. Defaults to null.</param>
    /// <returns>The formatted string with placeholders replaced.</returns>
    /// <exception cref="ArgumentNullException">Thrown when format is null.</exception>
    /// <example>
    /// var args = new[] { new KeyValuePair&lt;string, object&gt;("name", "Alice"), new KeyValuePair&lt;string, object&gt;("age", 30) };
    /// FormatWithNamedArgs("Name: {name}, Age: {age}", args) returns "Name: Alice, Age: 30"
    /// </example>
    public static string FormatWithNamedArgs(string format, IEnumerable<KeyValuePair<string, object>> args, object missingVal = null)
    {
        ArgumentNullException.ThrowIfNull(format);

        var d = args.NullSafeEnumerable().ToDictionary(z => z.Key, z => z.Value);

        return NameArgExpr.Replace(format, me => string.Format("{0" + me.Groups["modifiers"].Value + "}", d.GetValue(me.Groups["term"].Value, missingVal)));
    }
}
