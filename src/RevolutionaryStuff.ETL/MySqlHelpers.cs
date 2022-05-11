using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RevolutionaryStuff.ETL;

public static class MySqlHelpers
{
    private struct Term
    {
        public string Text;
        public bool Quoted;

        public override string ToString()
        {
            return string.Format("{0} quoted={1} text=[{2}]", GetType().Name, Quoted, Text);
        }

        public bool EqualsCaseInsensitive(string other)
        {
            return string.Compare(Text, other, true) == 0;
        }
    }

    public static DataSet LoadDump(Stream st)
    {
        Requires.ReadableStreamArg(st);
        var sr = new StreamReader(st);

        var ds = new DataSet();

        var terms = new List<Term>();

        var lastChar = ' ';
        var ch = ' ';
        var nextChar = ' ';
        var inSingleLineComment = false;
        var inMultiLineComment = false;
        var inSqlQuote = false;
        var inNameQuote = false;
        var skipOne = false;
        DataTable insertingTable = null;
        var parenthesisNestCount = 0;
        var sb = new StringBuilder();
        for (; ; )
        {
            lastChar = ch;
            ch = nextChar;
            var ich = sr.Read();
            if (ich == -1) break;
            nextChar = (char)ich;
            if (skipOne)
            {
                skipOne = false;
                continue;
            }
            switch (ch)
            {
                case '(':
                case ')':
                    if (!inSqlQuote && !inMultiLineComment && !inSqlQuote && !inNameQuote)
                    {
                        parenthesisNestCount += ch == '(' ? 1 : -1;
                    }
                    goto default;
                case '-':
                    if (inSqlQuote || inNameQuote || inMultiLineComment)
                    {
                        goto default;
                    }
                    else
                    {
                        if (nextChar == '-')
                        {
                            inSingleLineComment = true;
                        }
                    }
                    break;
                case '\r':
                case '\n':
                    if (inSingleLineComment)
                    {
                        inSingleLineComment = false;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '/':
                    if (inSqlQuote || inNameQuote || inSingleLineComment)
                    {
                        goto default;
                    }
                    else if (inMultiLineComment && lastChar == '*')
                    {
                        inMultiLineComment = false;
                    }
                    else if (!inMultiLineComment && nextChar == '*')
                    {
                        inMultiLineComment = true;
                        skipOne = true;
                    }
                    break;
                case '`':
                    if (inSqlQuote)
                    {
                        goto default;
                    }
                    else if (!inMultiLineComment && !inSingleLineComment)
                    {
                        if (inNameQuote)
                        {
                            if (nextChar == '`')
                            {
                                sb.Append(ch);
                                skipOne = true;
                            }
                            else
                            {
                                terms.Add(new Term { Text = sb.ToString(), Quoted = true });
                                sb.Clear();
                                inNameQuote = false;
                            }
                        }
                        else
                        {
                            inNameQuote = true;
                        }
                    }
                    break;
                case '\\':
                    if (inSqlQuote && nextChar == '\'')
                    {
                        sb.Append('\'');
                        skipOne = true;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '\'':
                    if (inNameQuote)
                    {
                        goto default;
                    }
                    else if (!inMultiLineComment && !inSingleLineComment)
                    {
                        if (inSqlQuote)
                        {
                            if (nextChar == '\'')
                            {
                                sb.Append(ch);
                                skipOne = true;
                            }
                            else
                            {
                                terms.Add(new Term { Text = sb.ToString(), Quoted = true });
                                sb.Clear();
                                inSqlQuote = false;
                            }
                        }
                        else
                        {
                            inSqlQuote = true;
                        }
                    }
                    break;
                default:
                    if (!inMultiLineComment && !inSingleLineComment)
                    {
                        var isWordChar = char.IsLetterOrDigit(ch) || ch is '_' or '.';
                        if (isWordChar || inSqlQuote || inNameQuote)
                        {
                            sb.Append(ch);
                        }
                        else
                        {
                            Term t;
                            var isWhiteSpace = char.IsWhiteSpace(ch);
                            if (sb.Length > 0)
                            {
                                terms.Add(new Term { Text = sb.ToString(), Quoted = false });
                                sb.Clear();
                            }
                            if (isWhiteSpace) continue;
                            t = new Term { Text = ch.ToString(), Quoted = false };
                            terms.Add(t);
                            if (t.Text == ";" && parenthesisNestCount == 0 && insertingTable != null)
                            {
                                Trace.WriteLine(string.Format("Staged {1} rows into table {0}", insertingTable.TableName, insertingTable.Rows.Count));
                                insertingTable = null;
                            }
                            else if (t.Text == ")" && parenthesisNestCount == 0)
                            {
                                if (insertingTable != null)
                                {
                                    for (var x = terms.Count - 1; x >= 0; --x)
                                    {
                                        t = terms[x];
                                        if (t.Text == "(" && t.Quoted == false)
                                        {
                                            var vals = new object[insertingTable.Columns.Count];
                                            ++x;
                                            for (var colNum = 0; colNum < vals.Length;)
                                            {
                                                t = terms[x++];
                                                if (t.Text == "," && !t.Quoted) continue;
                                                var dc = insertingTable.Columns[colNum++];
                                                if (t.Quoted == true || !t.EqualsCaseInsensitive("null"))
                                                {
                                                    object val;
                                                    try
                                                    {
                                                        val = Convert.ChangeType(t.Text, dc.DataType);
                                                        if (val is string && dc.MaxLength > 0 && ((string)val).Length > dc.MaxLength)
                                                        {
                                                            val = ((string)val).TruncateWithMidlineEllipsis(dc.MaxLength);
                                                        }
                                                        else if (val is DateTime)
                                                        {
                                                            if (((DateTime)val).Year < 1753)
                                                            {
                                                                if (dc.AllowDBNull)
                                                                {
                                                                    val = null;
                                                                }
                                                                else
                                                                {
                                                                    val = new DateTime(1753, 1, 1);
                                                                }
                                                            }
                                                            else if (((DateTime)val).Year > 9999)
                                                            {
                                                                val = new DateTime(9999, 12, 31);
                                                            }
                                                        }
                                                    }
                                                    catch (FormatException)
                                                    {
                                                        if (dc.AllowDBNull) continue;
                                                        throw;
                                                    }
                                                    vals[colNum - 1] = val;
                                                }
                                            }
                                            insertingTable.Rows.Add(vals);
                                            terms.Clear();
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    for (var x = terms.Count - 1; x >= 2; --x)
                                    {
                                        if (terms[x - 1].EqualsCaseInsensitive("create") && terms[x].EqualsCaseInsensitive("table"))
                                        {
                                            ProcessCreateTable(ds, terms, x - 1);
                                            terms.Clear();
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (t.Text == "(")
                            {
                                if (parenthesisNestCount == 1 && insertingTable == null && terms[terms.Count - 2].EqualsCaseInsensitive("values"))
                                {
                                    for (var x = terms.Count - 1; x >= 2; --x)
                                    {
                                        if (terms[x - 1].EqualsCaseInsensitive("insert") && terms[x].EqualsCaseInsensitive("into"))
                                        {
                                            insertingTable = ds.Tables[terms[x + 1].Text];
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
        return ds;
    }

    private static void ProcessCreateTable(DataSet ds, IList<Term> terms, int startAt)
    {
        if (!terms[startAt + 0].EqualsCaseInsensitive("create")) throw new Exception("We must be on a create table statement");
        if (!terms[startAt + 1].EqualsCaseInsensitive("table")) throw new Exception("We must be on a create table statement");
        var tableName = terms[startAt + 2];
        var dt = new DataTable(tableName.Text);
        ds.Tables.Add(dt);
        while (terms[startAt].Text != "(") ++startAt;
        for (++startAt; ;)
        {
            var fieldName = terms[startAt].Text;
            if (!terms[startAt].Quoted && (0 == string.Compare(fieldName, "primary", true) || 0 == string.Compare(fieldName, "unique", true) || 0 == string.Compare(fieldName, "key", true)))
            {
                return;
            }
            var typeName = terms[startAt + 1].Text;
            int? typeLength = null;
            startAt += 2;
            if (terms[startAt].Text == "(")
            {
                typeLength = int.Parse(terms[startAt + 1].Text);
                while (terms[startAt++].Text != ")") ;
            }
            var nullable = string.Compare(terms[startAt].Text, "NULL", true) == 0;
            var dc = new DataColumn(fieldName);
            if (nullable) dc.AllowDBNull = true;
            switch (typeName.ToLower())
            {
                case "bool":
                case "boolean":
                    dc.DataType = typeof(bool);
                    break;
                case "tinyint":
                    dc.DataType = typeof(byte);
                    break;
                case "smallint":
                    dc.DataType = typeof(short);
                    break;
                case "int":
                    dc.DataType = typeof(int);
                    break;
                case "bigint":
                    dc.DataType = typeof(long);
                    break;
                case "float":
                    dc.DataType = typeof(float);
                    break;
                case "double":
                    dc.DataType = typeof(double);
                    break;
                case "date":
                case "datetime":
                    dc.DataType = typeof(DateTime);
                    break;
                case "char":
                case "varchar":
                case "nvarchar":
                case "text":
                    dc.DataType = typeof(string);
                    if (typeLength != null) dc.MaxLength = typeLength.Value;
                    break;
                default:
                    dc.DataType = typeof(string);
                    break;
            }
            dt.Columns.Add(dc);
            for (; ; )
            {
                if (startAt >= terms.Count - 1) return;
                var t = terms[startAt++];
                if (!t.Quoted && t.Text == ")") return;
                if (!t.Quoted && t.Text == ",") break;
            }
        }
    }
}
