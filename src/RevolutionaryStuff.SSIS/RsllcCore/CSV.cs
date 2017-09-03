using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace RevolutionaryStuff.Core
{
    public static class CSV
    {
        public const char FieldDelimComma = ',';
        public const char QuoteChar = '"';

        private static readonly Regex Escapable = new Regex(
            string.Format(
                @"(^\s)|(\s$)|([{0}])",
                Regex.Escape(",'\"")),
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static string Format(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            if (Escapable.IsMatch(s))
            {
                return '"' + s.Replace("\"", "\"\"") + '"';
            }

            return s;
        }

        public static string ToCsv(this IEnumerable<string> l, bool eol = true)
        {
            return FormatLine(l, eol);
        }

        public static string FormatLine(DictionaryEntry de)
        {
            return FormatLine(new[] { de.Key, de.Value });
        }

        public static string FormatLine(IEnumerable l, bool eol = true)
        {
            var sb = new StringBuilder();
            FormatLine(sb, l, eol);
            return sb.ToString();
        }

        public static void FormatLine(StringBuilder sb, IEnumerable l)
        {
            FormatLine(sb, l, true);
        }

        public static void FormatLine(StringBuilder sb, IEnumerable l, bool eol = true)
        {
            int n = 0;

            if (l!=null)
            {
                foreach (object o in l)
                {
                    if (n++ > 0)
                    {
                        sb.Append(",");
                    }
                    if (null != o)
                    {
                        sb.Append(Format(o.ToString()));
                    }
                }
            }
            if (eol && n>0)
            {
                sb.Append("\r\n");
            }
        }

        public static int[] ParseIntegerRow(string rowOfIntsCsv)
        {
            var parsedText = ParseText(rowOfIntsCsv);

            if (parsedText == null || parsedText.Length == 0) return Empty.IntArray;

            string[] row = parsedText[0];
            var ret = new int[row.Length];
            for (int z = 0; z < ret.Length; ++z)
            {
                ret[z] = int.Parse(row[z]);
            }
            return ret;
        }

        public static T[] ParseRow<T>(string rowOfIntsCsv, Func<string, T> converter)
        {            
            var parsedText = ParseText(rowOfIntsCsv);
            if (parsedText == null || parsedText.Length == 0) return new T[0];

            var row = parsedText[0];
            var ret = new T[row.Length];
            for (int z = 0; z < ret.Length; ++z)
            {
                ret[z] = converter(row[z]);
            }
            return ret;
        }

        public static string[] ParseLine(string sText, char fieldDelim = FieldDelimComma)
        {
            if (string.IsNullOrEmpty(sText)) return Empty.StringArray;
            int amtParsed;
            return ParseLine(sText, 0, sText.Length, out amtParsed, fieldDelim);
        }

        private static string[] ParseLine(string sText, int start, int len, out int amtParsed, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
        {
            amtParsed = 0;
            int x = start;
            if (len == 0) return null;
            var b = new List<string>();
            var sb = new StringBuilder(1024 * 8);
            for (; x <= len + start;)
            {
                if (x > start)
                {
                    amtParsed = x - start;
                    return b.ToArray();
                }
                sb.Clear();
                bool inquotes = false;
                char ch;
                int sTextLen = sText.Length;
                for (; x < sTextLen; ++x)
                {
                    ch = sText[x];
                    switch (ch)
                    {
                        case '\r':
                            if (inquotes)
                            {
                                sb.Append(ch);
                            }
                            else
                            {
                                if (x < (start + len - 1) && sText[x + 1] == '\n')
                                {
                                    ++x;
                                }
                                ++x;
                                goto L_NextLine;
                            }
                            break;
                        case '\n':
                            if (inquotes)
                            {
                                sb.Append(ch);
                            }
                            else
                            {
                                ++x;
                                goto L_NextLine;
                            }
                            break;
                        default:
                            if (ch == fieldDelim)
                            {
                                if (inquotes)
                                {
                                    sb.Append(ch);
                                }
                                else
                                {
                                    //									DebugMessages.WriteLine(String.Format("x={0} field={1}", x, s));
                                    b.Add(sb.ToString());
                                    sb.Clear();
                                }
                                break;
                            }
                            else if (quoteChar.HasValue && ch == quoteChar)
                            {
                                if (inquotes)
                                {
                                    if (x < sTextLen - 1 && sText[x + 1] == ch)
                                    {
                                        sb.Append(ch);
                                        ++x;
                                    }
                                    else
                                    {
                                        inquotes = false;
                                    }
                                }
                                else
                                {
                                    inquotes = true;
                                }
                            }
                            else
                            {
                                sb.Append(ch);
                            }
                            break;
                    }
                }
                L_NextLine:
                //					DebugMessages.WriteLine(String.Format("x={0} field={1}", x, s));
                b.Add(sb.ToString());
                sb.Clear();
            }
            return null;
        }

        public static string[][] ParseText(string sText, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
        {
            return ParseTextEnumerable(sText, fieldDelim, quoteChar).ToArray();
        }

        public static IEnumerable<string[]> ParseTextEnumerable(string sText, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
        {
            int len = sText == null ? 0 : sText.Length;
            if (len > 0)
            {
                for (int start = 0; ;)
                {
                    int amt;
                    string[] line = ParseLine(sText, start, len, out amt, fieldDelim, quoteChar);
                    if (line == null) break;
                    yield return line;
                    start += amt;
                    len -= amt;
                }
            }
        }
    }
}