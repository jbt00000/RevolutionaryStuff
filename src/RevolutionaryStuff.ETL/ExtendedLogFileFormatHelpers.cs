using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RevolutionaryStuff.Data.ETL;

/// <remarks>http://www.w3.org/TR/WD-logfile.html</remarks>
public static class ExtendedLogFileFormatHelpers
{
    public static DataTable Load(Stream st, HashSet<string> skipCols = null)
    {
        Requires.ReadableStreamArg(st, "st");

        var dt = new DataTable();
        using var sr = new StreamReader(st);
        var sb = new StringBuilder(2048);
        string[] cols = null;
        DataRow row = null;
        var colNum = 0;
        var appendVal = delegate ()
        {
            var val = sb.ToString();
            sb.Clear();
            var colName = cols[colNum++];
            if (skipCols != null && skipCols.Contains(colName)) return;
            row[colName] = val;
        };
        for (var lineNum = 0; ; ++lineNum)
        {
            var line = sr.ReadLine();
            if (line == null) break;
            line = line.TrimOrNull();
            if (line == null) continue;
            if (line[0] == '#')
            {
                if (line.StartsWith("#Fields:"))
                {
                    line = line.RightOf("#Fields:").Trim();
                    cols = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    dt.Columns.AddRange(cols);
                }
            }
            else
            {
                try
                {
                    colNum = 0;
                    char ch, next;
                    var inQuotedString = false;
                    var isLastChar = false;
                    sb.Clear();
                    row = dt.NewRow();
                    for (var z = 0; z < line.Length; ++z)
                    {
                        isLastChar = z == line.Length - 1;
                        ch = line[z];
                        next = isLastChar ? ' ' : line[z + 1];
                        if (sb.Length == 0 && ch == '-' && char.IsWhiteSpace(next))
                        {
                            ++colNum;
                            sb.Clear();
                            ++z;
                        }
                        else if (sb.Length > 0 && char.IsWhiteSpace(ch) && !inQuotedString)
                        {
                            appendVal();
                        }
                        else if (ch == '"')
                        {
                            if (!inQuotedString && sb.Length == 0)
                            {
                                inQuotedString = true;
                            }
                            else if (!inQuotedString)
                            {
                                sb.Append(ch);
                            }
                            else if (inQuotedString && next == '"')
                            {
                                appendVal();
                                ++z;
                            }
                            else if (inQuotedString && char.IsWhiteSpace(next))
                            {
                                appendVal();
                                inQuotedString = false;
                            }
                            else
                            {
                                throw new FormatException($"In row {lineNum}");
                            }
                        }
                        else if (char.IsWhiteSpace(ch))
                        {
                            if (sb.Length > 0)
                            {
                                appendVal();
                            }
                        }
                        else
                        {
                            sb.Append(ch);
                            if (isLastChar)
                            {
                                appendVal();
                            }
                        }
                    }
                    if (colNum != cols.Length)
                    {
                        throw new FormatException($"In row {lineNum} we are jagged");
                    }
                    dt.Rows.Add(row);
                    if (dt.Rows.Count % 50000 == 0)
                    {
                        GC.Collect(5, GCCollectionMode.Forced, true);
                    }
                }
                catch (FormatException ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
        }

        return dt;
    }
}
