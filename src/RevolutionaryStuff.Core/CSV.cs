using System.Collections;
using System.IO;
using System.Text;

namespace RevolutionaryStuff.Core;

public static class CSV
{
    public const char FieldDelimPipe = '|';
    public const char FieldDelimComma = ',';
    public const char FieldDelimDefault = FieldDelimComma;

    public const char QuoteChar = '"';

    private static char[] CreateEscapeTriggers(char fieldDelim)
        => new[] { '\r', '\n', '\"', fieldDelim };

    private static readonly char[] CsvEscapeTrigger = CreateEscapeTriggers(FieldDelimComma);

    private static readonly char[] PipeEscapeTrigger = CreateEscapeTriggers(FieldDelimPipe);

    private static char[] FindOrCreateEscapeTrigger(char fieldDelim)
    {
        switch (fieldDelim)
        {
            case FieldDelimComma:
                return CsvEscapeTrigger;
            case FieldDelimPipe:
                return PipeEscapeTrigger;
            default:
                return CreateEscapeTriggers(fieldDelim);
        }
    }

    public static string Format(string s, char[] escapeTriggers = null)
    {
        if (string.IsNullOrEmpty(s)) return s;

        escapeTriggers ??= CsvEscapeTrigger;

        if (s.IndexOfAny(escapeTriggers) >= 0)
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

    public static void FormatLine(StringBuilder sb, IEnumerable l, bool eol = true, char fieldDelim = FieldDelimComma)
    {
        var n = 0;

        var escapeTrigger = FindOrCreateEscapeTrigger(fieldDelim);

        if (l != null)
        {
            foreach (var o in l)
            {
                if (n++ > 0)
                {
                    sb.Append(fieldDelim);
                }
                if (null != o)
                {
                    sb.Append(Format(o.ToString(), escapeTrigger));
                }
            }
        }
        if (eol && n > 0)
        {
            sb.Append("\r\n");
        }
    }

    public static int[] ParseIntegerRow(string rowOfIntsCsv)
    {
        var parsedText = ParseText(rowOfIntsCsv);

        if (parsedText == null || parsedText.Length == 0) return Empty.IntArray;

        var row = parsedText[0];
        var ret = new int[row.Length];
        for (var z = 0; z < ret.Length; ++z)
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
        for (var z = 0; z < ret.Length; ++z)
        {
            ret[z] = converter(row[z]);
        }
        return ret;
    }

    public static string[] ParseLine(string sText, char fieldDelim = FieldDelimComma)
    {
        if (string.IsNullOrEmpty(sText)) return Empty.StringArray;
        return ParseLine(sText, 0, sText.Length, out var amtParsed, fieldDelim);
    }

    private static string[] ParseLine(string sText, long start, long len, out long amtParsed, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
        => ParseLine(new StringCharacterReader(sText), start, len, out amtParsed, fieldDelim, quoteChar);

    private static string[] ParseLine(ICharacterReader sText, long start, long len, out long amtParsed, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar, StringBuilder sb = null)
    {
        amtParsed = 0;
        var x = start;
        if (len == 0) return null;
        var b = new List<string>();
        sb ??= new StringBuilder(1024 * 8);
        for (; x <= len + start;)
        {
            if (x > start)
            {
                amtParsed = x - start;
                return b.ToArray();
            }
            sb.Clear();
            var inquotes = false;
            char ch;
            var sTextLen = sText.Length;
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

    public static string[][] ParseText(StreamReader sr, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
    {
        var cr = new StreamReaderCharacterReader(sr);
        return ParseTextEnumerable(cr, fieldDelim, quoteChar).ToArray();
    }

    public static string[][] ParseText(string sText, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
        => ParseTextEnumerable(sText, fieldDelim, quoteChar).ToArray();

    public static IEnumerable<string[]> ParseTextEnumerable(string sText, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
        => ParseTextEnumerable(new StringCharacterReader(sText), fieldDelim, quoteChar);

    private static IEnumerable<string[]> ParseTextEnumerable(ICharacterReader sText, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
    {
        var sb = new StringBuilder(1024 * 8);
        var len = sText == null ? 0 : sText.Length;
        var rowCount = 0;
        if (len > 0)
        {
            for (long start = 0; ;)
            {
                long amt;
                var line = ParseLine(sText, start, len, out amt, fieldDelim, quoteChar, sb);
                if (line == null) break;
                ++rowCount;
                yield return line;
                start += amt;
                len -= amt;
            }
        }
    }

    private interface ICharacterReader
    {
        long Length { get; }
        char this[long index] { get; }
    }

    private class StringCharacterReader : ICharacterReader
    {
        private readonly string Text;

        public StringCharacterReader(string s)
        {
            Text = s ?? "";
        }

        char ICharacterReader.this[long index]
            => Text[(int)index];

        long ICharacterReader.Length
            => Text.Length;
    }

    private class StreamReaderCharacterReader : ICharacterReader
    {
        private StreamReader R;

        public StreamReaderCharacterReader(StreamReader sr)
        {
            sr ??= StreamReader.Null;
            Requires.True(sr.BaseStream.CanSeek, nameof(sr.BaseStream.CanSeek));
            Requires.True(sr.BaseStream.Position == 0, nameof(sr.BaseStream.Position));
            R = sr;
            while (sr.Read() != -1)
            {
                ++Length_p;
            }
            Reset();
        }

        private void Reset()
        {
            CharIndex = 0;
            R.BaseStream.Position = 0;
            R = new StreamReader(R.BaseStream, R.CurrentEncoding, true, 1024 * 1024, true);
        }

        private readonly long Length_p;
        long ICharacterReader.Length
            => Length_p;

        private long CharIndex = 0;
        char ICharacterReader.this[long index]
        {
            get
            {
                if (index < CharIndex)
                {
                    Reset();
                }
                while (index > CharIndex)
                {
                    CharIndex++;
                    R.Read();
                }
                return (char)R.Peek();
            }
        }
    }
}
