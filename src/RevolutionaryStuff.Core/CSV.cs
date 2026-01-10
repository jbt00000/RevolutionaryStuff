using System.Collections;
using System.IO;
using System.Text;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides utility methods for parsing and formatting CSV (Comma-Separated Values) data.
/// Supports custom delimiters (comma, pipe, etc.), quote escaping, and multi-line values.
/// </summary>
public static class CSV
{
    /// <summary>
    /// The pipe character '|' used as a field delimiter.
    /// </summary>
    public const char FieldDelimPipe = '|';

    /// <summary>
    /// The comma character ',' used as a field delimiter.
    /// </summary>
    public const char FieldDelimComma = ',';

    /// <summary>
    /// The default field delimiter (comma).
    /// </summary>
    public const char FieldDelimDefault = FieldDelimComma;

    /// <summary>
    /// The quote character '"' used for escaping field values containing delimiters, quotes, or newlines.
    /// </summary>
    public const char QuoteChar = '"';

    private static char[] CreateEscapeTriggers(char fieldDelim)
        => ['\r', '\n', '\"', fieldDelim];

    private static readonly char[] CsvEscapeTrigger = CreateEscapeTriggers(FieldDelimComma);

    private static readonly char[] PipeEscapeTrigger = CreateEscapeTriggers(FieldDelimPipe);

    private static char[] FindOrCreateEscapeTrigger(char fieldDelim)
    {
        return fieldDelim switch
        {
            FieldDelimComma => CsvEscapeTrigger,
            FieldDelimPipe => PipeEscapeTrigger,
            _ => CreateEscapeTriggers(fieldDelim),
        };
    }

    /// <summary>
    /// Formats a string value for CSV output, adding quotes and escaping as needed.
    /// Values containing delimiters, quotes, or newlines are automatically quoted.
    /// Internal quotes are escaped by doubling them ("" becomes "").
    /// </summary>
    /// <param name="s">The string to format.</param>
    /// <param name="escapeTriggers">
    /// Optional array of characters that trigger quoting. 
    /// If null, uses default triggers (newlines, quotes, and commas).
    /// </param>
    /// <returns>
    /// The formatted string, quoted and escaped if necessary. 
    /// Returns the original string if it doesn't contain any escape triggers.
    /// </returns>
    public static string Format(string s, char[] escapeTriggers = null)
    {
        if (string.IsNullOrEmpty(s)) return s;

        escapeTriggers ??= CsvEscapeTrigger;

        return s.IndexOfAny(escapeTriggers) >= 0 ? '"' + s.Replace("\"", "\"\"") + '"' : s;
    }

    /// <summary>
    /// Converts a collection of strings to a CSV line.
    /// </summary>
    /// <param name="l">The collection of strings to format as CSV.</param>
    /// <param name="eol">If <c>true</c>, appends a line ending (CRLF) to the output.</param>
    /// <returns>A CSV-formatted string.</returns>
    public static string ToCsv(this IEnumerable<string> l, bool eol = false)
    {
        return FormatLine(l, eol);
    }

    /// <summary>
    /// Formats a DictionaryEntry as a CSV line with key and value.
    /// </summary>
    /// <param name="de">The dictionary entry to format.</param>
    /// <returns>A CSV-formatted string with the key and value, including a line ending.</returns>
    public static string FormatLine(DictionaryEntry de)
    {
        return FormatLine(new[] { de.Key, de.Value }, true);
    }

    /// <summary>
    /// Formats a collection of objects as a CSV line.
    /// </summary>
    /// <param name="l">The collection of objects to format. Each object is converted to string via ToString().</param>
    /// <param name="eol">If <c>true</c>, appends a line ending (CRLF) to the output.</param>
    /// <returns>A CSV-formatted string.</returns>
    public static string FormatLine(IEnumerable l, bool eol)
    {
        var sb = new StringBuilder();
        FormatLine(sb, l, eol);
        return sb.ToString();
    }

    /// <summary>
    /// Formats a collection of objects as a CSV line into a StringBuilder, including a line ending.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    /// <param name="l">The collection of objects to format.</param>
    public static void FormatLine(StringBuilder sb, IEnumerable l)
    {
        FormatLine(sb, l, true);
    }

    /// <summary>
    /// Formats a collection of objects as a CSV line into a StringBuilder.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    /// <param name="l">The collection of objects to format. Each object is converted to string via ToString().</param>
    /// <param name="eol">If <c>true</c>, appends a line ending (CRLF) to the output.</param>
    /// <param name="fieldDelim">The character to use as a field delimiter. Defaults to comma.</param>
    public static void FormatLine(StringBuilder sb, IEnumerable l, bool eol, char fieldDelim = FieldDelimComma)
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

    /// <summary>
    /// Parses a single row of comma-separated integers.
    /// </summary>
    /// <param name="rowOfIntsCsv">A CSV string containing integer values.</param>
    /// <returns>
    /// An array of parsed integers. Returns an empty array if the input is null or empty.
    /// </returns>
    /// <exception cref="FormatException">Thrown if a value cannot be parsed as an integer.</exception>
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

    /// <summary>
    /// Parses a single row of CSV values and converts them using a custom converter function.
    /// </summary>
    /// <typeparam name="T">The type to convert each field to.</typeparam>
    /// <param name="rowOfIntsCsv">A CSV string containing values to parse.</param>
    /// <param name="converter">Function to convert each string field to type T.</param>
    /// <returns>
    /// An array of converted values. Returns an empty array if the input is null or empty.
    /// </returns>
    public static T[] ParseRow<T>(string rowOfIntsCsv, Func<string, T> converter)
    {
        var parsedText = ParseText(rowOfIntsCsv);
        if (parsedText == null || parsedText.Length == 0) return [];

        var row = parsedText[0];
        var ret = new T[row.Length];
        for (var z = 0; z < ret.Length; ++z)
        {
            ret[z] = converter(row[z]);
        }
        return ret;
    }

    /// <summary>
    /// Parses a single CSV line into an array of field values.
    /// Handles quoted fields, escaped quotes (""), and embedded newlines within quotes.
    /// </summary>
    /// <param name="sText">The CSV line to parse.</param>
    /// <param name="fieldDelim">The character used as a field delimiter. Defaults to comma.</param>
    /// <returns>
    /// An array of field values. Returns an empty array if the input is null or empty.
    /// </returns>
    public static string[] ParseLine(string sText, char fieldDelim = FieldDelimComma)
    {
        return string.IsNullOrEmpty(sText) ? Empty.StringArray : ParseLine(sText, 0, sText.Length, out _, fieldDelim);
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

                        if (quoteChar.HasValue && ch == quoteChar)
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

    /// <summary>
    /// Parses CSV text from a StreamReader into a 2D array of strings.
    /// </summary>
    /// <param name="sr">The StreamReader containing CSV data.</param>
    /// <param name="fieldDelim">The character used as a field delimiter. Defaults to comma.</param>
    /// <param name="quoteChar">
    /// The character used for quoting field values. If null, quoting is disabled.
    /// Defaults to double-quote (").
    /// </param>
    /// <returns>
    /// A 2D array where each row is an array of field values.
    /// Each inner array represents one CSV line.
    /// </returns>
    public static string[][] ParseText(StreamReader sr, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
    {
        var cr = new StreamReaderCharacterReader(sr);
        return ParseTextEnumerable(cr, fieldDelim, quoteChar).ToArray();
    }

    /// <summary>
    /// Parses CSV text from a string into a 2D array of strings.
    /// </summary>
    /// <param name="sText">The CSV text to parse.</param>
    /// <param name="fieldDelim">The character used as a field delimiter. Defaults to comma.</param>
    /// <param name="quoteChar">
    /// The character used for quoting field values. If null, quoting is disabled.
    /// Defaults to double-quote (").
    /// </param>
    /// <returns>
    /// A 2D array where each row is an array of field values.
    /// Each inner array represents one CSV line.
    /// </returns>
    public static string[][] ParseText(string sText, char fieldDelim = FieldDelimComma, char? quoteChar = QuoteChar)
        => ParseTextEnumerable(sText, fieldDelim, quoteChar).ToArray();

    /// <summary>
    /// Parses CSV text from a string as an enumerable sequence of rows.
    /// This method provides lazy evaluation, useful for large CSV files.
    /// </summary>
    /// <param name="sText">The CSV text to parse.</param>
    /// <param name="fieldDelim">The character used as a field delimiter. Defaults to comma.</param>
    /// <param name="quoteChar">
    /// The character used for quoting field values. If null, quoting is disabled.
    /// Defaults to double-quote (").
    /// </param>
    /// <returns>
    /// An enumerable sequence where each element is an array of field values representing one CSV line.
    /// </returns>
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
                var line = ParseLine(sText, start, len, out var amt, fieldDelim, quoteChar, sb);
                if (line == null) break;
                ++rowCount;
                yield return line;
                start += amt;
                len -= amt;
            }
        }
    }

    /// <summary>
    /// Internal interface for reading characters from different sources (string or stream).
    /// </summary>
    private interface ICharacterReader
    {
        /// <summary>
        /// Gets the total length of the character source.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Gets the character at the specified index.
        /// </summary>
        char this[long index] { get; }
    }

    /// <summary>
    /// Character reader implementation for strings.
    /// </summary>
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

    /// <summary>
    /// Character reader implementation for StreamReader.
    /// Requires a seekable stream positioned at the beginning.
    /// </summary>
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
