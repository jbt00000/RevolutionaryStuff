using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace RevolutionaryStuff.Data.ETL;

public static partial class DataLoadingHelpers
{
    extension(DataTable source)
    {
        public void SetColumnWithValue<T>(string columnName, T value)
        {
            source.SetColumnWithValue(columnName, (a, b) => value);
        }
        public void SetColumnWithValue<T>(string columnName, Func<DataRow, int, T> valueGenerator)
        {
            ArgumentNullException.ThrowIfNull(source);
            Requires.Text(columnName);
            ArgumentNullException.ThrowIfNull(valueGenerator);

            var pos = source.Columns[columnName].Ordinal;
            for (var z = 0; z < source.Rows.Count; ++z)
            {
                var dr = source.Rows[z];
                var val = valueGenerator(dr, z);
                dr[pos] = val;
            }
        }
        public DataTable LoadRowsFromFixedWidthText(Stream st, LoadRowsFromFixedWidthTextSettings settings)
        {
            source ??= new DataTable();
            Requires.ZeroColumns(source);
            Requires.ZeroRows(source);
            Requires.ReadableStreamArg(st);
            Requires.Valid(settings);

            foreach (var f in settings.ColumnInfos)
            {
                source.Columns.Add(f.ColumnName, f.DataType ?? typeof(string));
            }
            IList<string[]> rows = null;
            using (var sr = new StreamReader(st))
            {
                for (; ; )
                {
                    var line = sr.ReadLine();
                    if (line == null) break;
                    var row = new string[settings.ColumnInfos.Count];
                    var x = 0;
                    foreach (var f in settings.ColumnInfos)
                    {
                        string s;
                        if (f.Length != null)
                        {
                            s = line.Substring(f.StartAt, f.Length.Value);
                        }
                        else if (f.EndAt != null)
                        {
                            s = line.Substring(f.StartAt, f.EndAt.Value - f.StartAt + 1);
                        }
                        else
                        {
                            s = line[f.StartAt..];
                        }
                        row[x++] = s;
                    }
                    rows.Add(row);
                }
            }
            GC.Collect();
            return source.LoadRowsInternal(rows, settings, false);
        }
        public DataTable LoadRowsFromJObjects(IEnumerable<JObject> items)
        {
            source ??= new DataTable();
            Requires.ZeroColumns(source);
            Requires.ZeroRows(source);
            Requires.HasData(items);

            var colByName = new Dictionary<string, DataColumn>(Comparers.CaseInsensitiveStringComparer);
            foreach (var item in items)
            {
                foreach (var p in item.Properties())
                {
                    if (colByName.ContainsKey(p.Name)) continue;
                    if (p.Value.Type == JTokenType.Null) continue;
                    var col = new DataColumn(p.Name, p.Value.Type.ToSimpleClrType());
                    colByName[col.ColumnName] = col;
                    source.Columns.Add(col);
                }
                var row = source.NewRow();
                foreach (var p in item.Properties())
                {
                    var col = colByName.FindOrDefault(p.Name);
                    var val = p.ToSimpleVal();
                    if (val == null) continue;
                    row[col] = val;
                }
                source.Rows.Add(row);
            }
            return source;
        }
        public DataTable LoadRowsFromObjects(System.Collections.IEnumerable items, LoadRowsFromObjectsSettings settings)
        {
            source ??= new DataTable();
            Requires.ZeroColumns(source);
            Requires.ZeroRows(source);
            Requires.Valid(settings);
            Requires.HasData(items);

            var memberFlags =
                BindingFlags.Public |
                BindingFlags.Instance |
                (settings.GetFieldsFromRelection ? BindingFlags.GetField : 0) |
                (settings.GetPropertiesFromRelection ? BindingFlags.GetProperty : 0);
            var n = 0;
            var colByName = new Dictionary<string, DataColumn>(Comparers.CaseInsensitiveStringComparer);
            foreach (var item in items)
            {
                var itemType = item.GetType();
                var mis = itemType.GetMembers(memberFlags);
                if (n == 0 || settings.ColumnsFromEachObject)
                {
                    foreach (var mi in mis)
                    {
                        if (colByName.ContainsKey(mi.Name)) continue;
                        var col = new DataColumn(mi.Name, mi.GetUnderlyingType());
                        colByName[col.ColumnName] = col;
                        source.Columns.Add(col);
                    }
                }
                var row = source.NewRow();
                foreach (var mi in mis)
                {
                    var col = colByName.FindOrDefault(mi.Name);
                    if (col == null) continue;
                    var val = mi.GetValue(item);
                    row[col] = val;
                }
                source.Rows.Add(row);
                ++n;
            }
            return source;
        }
        public DataTable LoadRowsFromDelineatedText(Stream st, LoadRowsFromDelineatedTextSettings settings)
        {
            source ??= new DataTable();
            Requires.ZeroColumns(source);
            Requires.ZeroRows(source);
            Requires.ReadableStreamArg(st);
            Requires.Valid(settings);

            IList<string[]> rows = null;
            if (settings.Format == LoadRowsFromDelineatedTextFormats.PipeSeparatedValues)
            {
                var sections = new List<string[]>();
                var lineNum = 0;
                using (var sr = new StreamReader(st))
                {
                    var maxCapacity = 1024 * 1024 * 127;
                    var sb = new StringBuilder(maxCapacity + (1024 * 1024));
                    for (; ; )
                    {
                        var line = sr.ReadLine();
                        if (line != null)
                        {
                            ++lineNum;
                            sb.AppendLine(line);
                        }
                        if (sb.Length > maxCapacity || line == null)
                        {
                            var csv = sb.ToString();
                            sb.Clear();
                            rows = CSV.ParseText(csv, '|', null);
                            csv = null;
                            sections.AddRange(rows);
                            rows = null;
                            if (line == null) break;
                        }
                    }
                }
                rows = sections;
            }
            else if (settings.Format == LoadRowsFromDelineatedTextFormats.CommaSeparatedValues)
            {
                using var sr = new StreamReader(st);
                rows = CSV.ParseText(sr);
            }
            else if (settings.Format == LoadRowsFromDelineatedTextFormats.Custom)
            {
                using var sr = new StreamReader(st);
                rows = CSV.ParseText(sr, settings.CustomFieldDelim, settings.CustomQuoteChar);
            }
            else
            {
                throw new UnexpectedSwitchValueException(settings.Format);
            }
            Trace.WriteLine($"Parsed {rows.Count} raw rows from raw text");
            GC.Collect();
            if (settings.ColumnNames != null && settings.ColumnNames.Length > 0)
            {
                source = source.LoadRowsInternal(PrependColumnNames(settings.ColumnNames, rows.Skip(settings.SkipRawRows)), settings);
            }
            else if (!string.IsNullOrEmpty(settings.ColumnNameTemplate))
            {
                var names = new List<string>();
                for (var z = 0; z < rows[0].Length; ++z)
                {
                    var name = string.Format(settings.ColumnNameTemplate, z);
                    names.Add(name);
                }
                source = source.LoadRowsInternal(PrependColumnNames(names, rows.Skip(settings.SkipRawRows)), settings);
            }
            else
            {
                source = source.LoadRowsInternal(rows.Skip(settings.SkipRawRows), settings);
            }
            Trace.WriteLine($"Loaded {source.Rows.Count} into the datatable");
            return source;
        }
        internal DataTable LoadRowsInternal(IEnumerable<IList<object>> rows, LoadRowsSettings settings, bool headerRowEmbedded = true)
        {
            Requires.ZeroRows(source);
            ArgumentNullException.ThrowIfNull(rows);
            settings ??= new LoadRowsSettings();
            if (!((headerRowEmbedded && source.Columns.Count == 0) || (!headerRowEmbedded && source.Columns.Count > 0)))
            {
                throw new InvalidOperationException("Header row must be embedded or table must already have columns");
            }

            using var e = rows.GetEnumerator();
            if (!e.MoveNext()) return source;

            var createColumns = source.Columns.Count == 0;
            DataColumn rowNumberColumn = null;
            if (settings.RowNumberColumnName != null)
            {
                rowNumberColumn = source.Columns[settings.RowNumberColumnName];
                if (rowNumberColumn == null)
                {
                    rowNumberColumn = new DataColumn(settings.RowNumberColumnName, typeof(int)) { AllowDBNull = false };
                    source.Columns.Add(rowNumberColumn);
                }
                else
                {
                    if (!(rowNumberColumn.DataType == typeof(int) || rowNumberColumn.DataType == typeof(long)))
                    {
                        throw new InvalidOperationException(string.Format("Existing table has a rowNumberColumn of an incompatible data type"));
                    }
                }
            }

            DataColumn[] columnMap;
            if (headerRowEmbedded)
            {
                var headerRow = e.Current;
                var columnMapper = settings.ColumnMapper ?? OneToOneColumnNameMapper;
                var duplicateColumnRenamer = settings.DuplicateColumnRenamer ?? OnDuplicateColumnNameThrow;
                columnMap = new DataColumn[headerRow.Count];
                for (var z = 0; z < headerRow.Count; ++z)
                {
                    var colName = Stuff.ObjectToString(headerRow[z]).TrimOrNull();
                    if (colName == null)
                    {
                        continue;
                    }
                    colName = columnMapper(colName);
                    if (colName == null)
                    {
                        continue;
                    }
                    var c = source.Columns[colName];
                    if (createColumns)
                    {
                        if (c == null)
                        {
                            c = new DataColumn(colName);
                            source.Columns.Add(c);
                        }
                        else
                        {
                            colName = duplicateColumnRenamer(source, colName);
                            c = new DataColumn(colName);
                            source.Columns.Add(c);
                        }
                    }
                    else if (c == null)
                    {
                        Trace.WriteLine($"Will ignore source column #{z} with name=[{colName}]");
                    }
                    columnMap[z] = c;
                }
                if (!e.MoveNext()) return source;
            }
            else
            {
                columnMap = source.Columns.OfType<DataColumn>().ToArray();
            }

            var rowNum = -1;
            var onRowAddError = settings.RowAddErrorHandler ?? RowAddErrorRethrow;
            do
            {
                ++rowNum;
                var row = e.Current;
                if (row.Count == 0) continue;
                var fields = new object[source.Columns.Count];
                try
                {
                    for (var z = 0; z < columnMap.Length; ++z)
                    {
                        var c = columnMap[z];
                        if (c == null) continue;
                        var val = z >= row.Count ? null : row[z];
                        if (val == null)
                        {
                            val = DBNull.Value;
                        }
                        else if (val.GetType() != c.DataType)
                        {
                            val = settings.TypeConverter(val, c.DataType);
                        }
                        fields[c.Ordinal] = val;
                    }
                    if (rowNumberColumn != null)
                    {
                        fields[rowNumberColumn.Ordinal] = rowNum;
                    }
                    if (null == settings.ShouldAddRow || settings.ShouldAddRow(source, fields))
                    {
                        source.Rows.Add(fields);
                    }
                }
                catch (Exception ex)
                {
                    onRowAddError(ex, rowNum);
                }
            }
            while (e.MoveNext());

            return source;
        }
        public void Append(DataTable other, bool appendOtherTableColumns = false)
        {
            ArgumentNullException.ThrowIfNull(other);

            var sameStructure = source.Columns.Count == other.Columns.Count;
            var dtWasBlank = source.Columns.Count == 0;
            var columnNamesToAppend = new List<string>();
            foreach (DataColumn bCol in other.Columns)
            {
                var aCol = source.Columns[bCol.ColumnName];
                sameStructure = sameStructure && aCol != null && aCol.Ordinal == bCol.Ordinal;
                if (aCol == null)
                {
                    if (!appendOtherTableColumns) continue;
                    source.Columns.Add(bCol.CloneToUnbound());
                }
                columnNamesToAppend.Add(bCol.ColumnName);
            }
            sameStructure = (sameStructure || dtWasBlank) && (source.Columns.Count == other.Columns.Count);
            foreach (DataRow bRow in other.Rows)
            {
                if (sameStructure)
                {
                    source.Rows.Add(bRow.ItemArray);
                }
                else
                {
                    var aRow = source.NewRow();
                    foreach (var columnName in columnNamesToAppend)
                    {
                        aRow[columnName] = bRow[columnName];
                    }
                    source.Rows.Add(aRow);
                }
            }
        }
    }

    public static void RowAddErrorIgnore(Exception ex, int rowNum)
    {
        Trace.WriteLine($"Problem adding row {rowNum} because [{ex.Message}]");
    }

    public static void RowAddErrorRethrow(Exception ex, int rowNum)
    {
        throw new Exception($"Problem adding row {rowNum}", ex);
    }

    public static bool DontAddEmptyRows(DataTable dt, object[] row)
    {
        foreach (var v in row)
        {
            if (v != DBNull.Value && v != null && v.ToString() != "") return true;
        }
        return false;
    }

    public static void RowAddErrorTraceAndIgnore(Exception ex, int rowNum)
    {
        Trace.WriteLine($"Problem adding row {rowNum}.  Will Skip.\n{ex}");
    }

    public static DataTable LoadRowsFromFixedWidthText(Stream st, LoadRowsFromFixedWidthTextSettings settings)
        => new DataTable().LoadRowsFromFixedWidthText(st, settings);

    extension(JTokenType source)
    {
        private Type ToSimpleClrType()
        {
            switch (source)
            {
                case JTokenType.Null:
                    return typeof(string);
                case JTokenType.String:
                    return typeof(string);
                case JTokenType.Boolean:
                    return typeof(bool);
                case JTokenType.Integer:
                    return typeof(int);
                case JTokenType.Float:
                    return typeof(float);
                case JTokenType.Date:
                    return typeof(DateTime);
                case JTokenType.Guid:
                    return typeof(Guid);
                default:
                    throw new NotSupportedException($"No conversion for {source}");
            }
        }
    }

    extension(JProperty source)
    {
        private object ToSimpleVal()
        {
            switch (source.Value.Type)
            {
                case JTokenType.Null:
                    return null;
                case JTokenType.String:
                    return source.Value.Value<string>();
                case JTokenType.Boolean:
                    return source.Value.Value<bool>();
                case JTokenType.Integer:
                    return source.Value.Value<int>();
                case JTokenType.Float:
                    return source.Value.Value<float>();
                case JTokenType.Date:
                    return source.Value.Value<DateTime>();
                case JTokenType.Guid:
                    return source.Value.Value<Guid>();
                default:
                    throw new NotSupportedException($"No conversion for {source.Type}");
            }
        }
    }

    public static DataTable LoadRowsFromDelineatedText(Stream st, LoadRowsFromDelineatedTextSettings settings)
        => new DataTable().LoadRowsFromDelineatedText(st, settings);

    private static IEnumerable<IList<object>> PrependColumnNames(IEnumerable<string> columnNames, IEnumerable<IList<object>> rows)
    {
        var cn = columnNames.OfType<object>().ToList();
        yield return cn;
        foreach (var row in rows)
        {
            yield return row;
        }
    }

    extension(DataColumn source)
    {
        public DataColumn CloneToUnbound()
                => new(source.ColumnName, source.DataType)
                {
                    AllowDBNull = source.AllowDBNull,
                    Caption = source.Caption,
                    AutoIncrement = source.AutoIncrement,
                    AutoIncrementSeed = source.AutoIncrementSeed,
                    AutoIncrementStep = source.AutoIncrementStep,
                    DateTimeMode = source.DateTimeMode,
                    DefaultValue = source.DefaultValue,
                    Expression = source.Expression,
                    MaxLength = source.MaxLength,
                    Namespace = source.Namespace,
                    Prefix = source.Prefix,
                    ReadOnly = source.ReadOnly,
                    Unique = source.Unique
                };
    }

    #region Data Column Helpers

    public static string OneToOneColumnNameMapper(string inboundColumnName)
    {
        return inboundColumnName;
    }

    private static readonly Regex MakeFriendlyExpr = new("[^0-9a-z]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string UpperCamelNoSpecialCharactersColumnNameMapper(string inboundColumnName)
    {
        var s = MakeFriendlyExpr.Replace(inboundColumnName, " ");
        s = s.ToUpperCamelCase();
        return s;
    }

    public static Func<string, string> CreateDictionaryMapper(IDictionary<string, string> m, bool onMissingPassthrough = false)
    {
        ArgumentNullException.ThrowIfNull(m);
        var f = delegate (string s)
        {
            if (m.ContainsKey(s)) return m[s];
            return onMissingPassthrough ? s : null;
        };
        return f;
    }

    public static string OnDuplicateColumnNameThrow(DataTable dt, string duplicateColumnName)
    {
        throw new Exception(
            $"Datatable cannot have duplicate column names.  [{duplicateColumnName}] occurs at least twice");
    }

    public static string OnDuplicateAppendSeqeuntialNumber(DataTable dt, string inboundColumnName)
    {
        for (var z = 2; ; ++z)
        {
            var newName = $"{inboundColumnName}_{z}";
            if (!dt.Columns.Contains(newName)) return newName;
        }
    }

    #endregion
}
