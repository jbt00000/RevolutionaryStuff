//using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core
{
    public static class RegexHelpers
    {
        public const RegexOptions IgnoreCaseSingleLineCompiled = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
/*
        private static readonly ICache<string, Regex> RegexCache = Cache.CreateSynchronized<string, Regex>(1024);

        public static Regex Create(string pattern, RegexOptions options = RegexOptions.None)
        {
            options = options | RegexOptions.Compiled;
            var key = string.Format("{0};{1}", options, pattern);
            return RegexCache.Do(key, () => new Regex(pattern, options));
        }
*/
        public static class Common
        {
            public static readonly Regex CSharpIdentifier = new Regex(@"^[a-z][a-z0-9_]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            /// <remarks>
            /// Conforms to RCF 2822 
            /// http://dubinko.info/writing/xforms/book.html#id2848057
            /// </remarks>
            public static readonly Regex EmailAddress = new Regex(@"[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+(\.[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+)*@[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+(\.[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+)*", RegexOptions.Compiled);

            public static readonly Regex Space = new Regex(" ", RegexOptions.Compiled);

            public static readonly Regex Whitespace = new Regex(@"\s", RegexOptions.Compiled);
            public static readonly Regex N = new Regex("\n", RegexOptions.Compiled);
            public static readonly Regex NN = new Regex("\n\n", RegexOptions.Compiled);
            public static readonly Regex NullJsonMember = new Regex(@"(""\w+""\s*:\s*null\s*,?)|(,?\s*""\w+""\s*:\s*null\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
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
    }
}