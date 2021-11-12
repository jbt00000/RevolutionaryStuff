using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core;

public static class RegexHelpers
{
    public const RegexOptions IgnoreCaseSingleLineCompiled = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;

    public static Regex Create(string pattern, RegexOptions options = RegexOptions.None)
        => Cache.DataCacher.FindOrCreateValue(
            Cache.CreateKey(typeof(RegexHelpers), nameof(Create), pattern, options | RegexOptions.Compiled),
            () => new Regex(pattern, options | RegexOptions.Compiled));

    public static class Common
    {
        public static readonly Regex CSharpIdentifier = new Regex(@"^[_a-z][a-z0-9_]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <remarks>
        /// Conforms to RCF 2822 
        /// http://dubinko.info/writing/xforms/book.html#id2848057
        /// </remarks>
        public static readonly Regex EmailAddress = new Regex(@"[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+(\.[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+)*@[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+(\.[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+)*", RegexOptions.Compiled);

        public static readonly Regex Space = new Regex(" ", RegexOptions.Compiled);

        public static readonly Regex NonDigits = new Regex(@"\D", RegexOptions.Compiled);
        public static readonly Regex Digits = new Regex(@"\d", RegexOptions.Compiled);
        public static readonly Regex WordChars = new Regex(@"\w", RegexOptions.Compiled);
        public static readonly Regex NonWordChars = new Regex(@"\W", RegexOptions.Compiled);
        public static readonly Regex Whitespace = new Regex(@"\s", RegexOptions.Compiled);
        public static readonly Regex N = new Regex("\n", RegexOptions.Compiled);
        public static readonly Regex NN = new Regex("\n\n", RegexOptions.Compiled);
        public static readonly Regex NullJsonMember = new Regex(@"(""\w+""\s*:\s*null\s*,?)|(,?\s*""\w+""\s*:\s*null\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex InvalidPathChars =
            new Regex(
                String.Format(
                    @"([\*\?{0}])",
                    Regex.Escape(new string(Path.GetInvalidPathChars()))
                    ),
                RegexOptions.Compiled
                );
    }

    public static IList<string> GetMatchesGroupValue(this Regex r, string input, int groupNum = 1)
    {
        var vals = new List<string>();
        foreach (Match m in r.Matches(input))
        {
            if (m.Success)
            {
                vals.Add(m.Groups[groupNum].Value);
            }
        }
        return vals;
    }

    public static string GetGroupValue(this Regex r, string input, string fallback = null, int groupNum = 1)
    {
        try
        {
            var m = r.Match(input);
            if (m.Success)
            {
                return m.Groups[groupNum].Value;
            }
        }
        catch (Exception) { }
        return fallback;
    }

    public static IList<string> GetGroupValues(this Regex r, string input)
    {
        try
        {
            var m = r.Match(input);
            if (m.Success)
            {
                var ret = new List<string>(m.Groups.Count - 1);
                for (int z = 1; z < m.Groups.Count; ++z)
                {
                    ret[z - 1] = m.Groups[z].Value;
                }
                return ret;
            }
        }
        catch (Exception) { }
        return Empty.StringArray;
    }

    public static string Replace(this string s, Regex r, Func<(Match Match, int Occurrence, string InputString, string CurrentString), string> replacer)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var input = s;
        int cnt = 0;
        for (int startAt = 0; ;)
        {
            var m = r.Match(s, startAt);
            if (!m.Success) break;
            string replacement = replacer == null ? "" : replacer((m, cnt++, input, s));
            s = s.Substring(0, m.Index) + replacement + s.Substring(m.Index + m.Value.Length);
            startAt = m.Index + replacement.Length;
        }
        return s;
    }
}
