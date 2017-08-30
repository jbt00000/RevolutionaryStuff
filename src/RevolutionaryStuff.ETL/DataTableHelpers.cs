using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.ETL
{
    public static partial class DataTableHelpers
    {
        internal static void CreateColumns(this DataTable dt, IEnumerable<string> fieldNames)
        {
            var existing = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            foreach (DataColumn dc in dt.Columns)
            {
                existing.Add(dc.ColumnName);
            }
            foreach (var colName in fieldNames)
            {
                if (existing.Contains(colName)) continue;
                existing.Add(colName);
                dt.Columns.Add(colName);
            }
        }
          
        public static void SetColumnWithValue<T>(this DataTable dt, string columnName, T value)
        {
            dt.SetColumnWithValue(columnName, (a, b) => value);
        }

        public static void SetColumnWithValue<T>(this DataTable dt, string columnName, Func<DataRow, int, T> valueGenerator)
        {
            Requires.NonNull(dt, nameof(dt));
            Requires.Text(columnName, nameof(columnName));
            Requires.NonNull(valueGenerator, nameof(valueGenerator));

            var pos = dt.Columns[columnName].Ordinal;
            for (int z = 0; z < dt.Rows.Count; ++z)
            {
                var dr = dt.Rows[z];
                var val = valueGenerator(dr, z);
                dr[pos] = val;
            }
        }

        public static void RowAddErrorIgnore(Exception ex, int rowNum)
        {
            Trace.WriteLine(string.Format("Problem adding row {0} because [{1}]", rowNum, ex.Message));
        }

        public static void RowAddErrorRethrow(Exception ex, int rowNum)
        {
            throw new Exception(string.Format("Problem adding row {0}", rowNum), ex);
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
            Trace.WriteLine(string.Format("Problem adding row {0}.  Will Skip.\n{1}", rowNum, ex));
        }

        public static DataTable LoadRowsFromFixedWidthText(Stream st, LoadRowsFromFixedWidthTextSettings settings)
            => new DataTable().LoadRowsFromFixedWidthText(st, settings);

        public static DataTable LoadRowsFromFixedWidthText(this DataTable dt, Stream st, LoadRowsFromFixedWidthTextSettings settings)
        {
            dt = dt ?? new DataTable();
            Requires.ZeroColumns(dt, nameof(dt));
            Requires.ZeroRows(dt, nameof(dt));
            Requires.ReadableStreamArg(st, nameof(st));
            Requires.Valid(settings, nameof(settings));

            foreach (var f in settings.ColumnInfos)
            {
                dt.Columns.Add(f.ColumnName, f.DataType ?? typeof(string));
            }
            IList<string[]> rows = null;
            using (var sr = new StreamReader(st))
            {
                for (;;)
                {
                    var line = sr.ReadLine();
                    if (line == null) break;
                    var row = new string[settings.ColumnInfos.Count];
                    int x = 0;
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
                            s = line.Substring(f.StartAt);
                        }
                        row[x++] = s;
                    }
                    rows.Add(row);
                }
            }
            GC.Collect();
            return dt.LoadRowsInternal(rows, settings, false);
        }

        public static DataTable LoadRowsFromDelineatedText(Stream st, LoadRowsFromDelineatedTextSettings settings)
            => new DataTable().LoadRowsFromDelineatedText(st, settings);

        public static DataTable LoadRowsFromDelineatedText(this DataTable dt, Stream st, LoadRowsFromDelineatedTextSettings settings)
        {
            dt = dt ?? new DataTable();
            Requires.ZeroColumns(dt, nameof(dt));
            Requires.ZeroRows(dt, nameof(dt));
            Requires.ReadableStreamArg(st, nameof(st));
            Requires.Valid(settings, nameof(settings));

            IList<string[]> rows = null;
            if (settings.Format == LoadRowsFromDelineatedTextFormats.PipeSeparatedValues)
            {
                var sections = new List<string[]>();
                int lineNum = 0;
                using (var sr = new StreamReader(st))
                {
                    var maxCapacity = 1024 * 1024 * 127;
                    var sb = new StringBuilder(maxCapacity + 1024 * 1024);
                    for (;;)
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
                var data = new StreamReader(st).ReadToEnd();
                rows = CSV.ParseText(data);
            }
            else if (settings.Format == LoadRowsFromDelineatedTextFormats.Custom)
            {
                var data = new StreamReader(st).ReadToEnd();
                rows = CSV.ParseText(data, settings.CustomFieldDelim, settings.CustomQuoteChar);
            }
            else
            {
                throw new UnexpectedSwitchValueException(settings.Format);
            }
            GC.Collect();
            if (settings.ColumnNames != null && settings.ColumnNames.Length > 0)
            {
                return dt.LoadRowsInternal(PrependColumnNames(settings.ColumnNames, rows.Skip(settings.SkipRawRows)), settings);
            }
            else if (!string.IsNullOrEmpty(settings.ColumnNameTemplate))
            {
                var names = new List<string>();
                for (int z = 0; z < rows[0].Length; ++z)
                {
                    var name = string.Format(settings.ColumnNameTemplate, z);
                    names.Add(name);
                }
                return dt.LoadRowsInternal(PrependColumnNames(names, rows.Skip(settings.SkipRawRows)), settings);
            }
            else
            {
                return dt.LoadRowsInternal(rows.Skip(settings.SkipRawRows), settings);
            }
        }

        private static IEnumerable<IList<object>> PrependColumnNames(IEnumerable<string> columnNames, IEnumerable<IList<object>> rows)
        {
            var cn = columnNames.OfType<object>().ToList();
            yield return cn;
            foreach (var row in rows)
            {
                yield return row;
            }
        }

        internal static DataTable LoadRowsInternal(this DataTable dt, IEnumerable<IList<object>> rows, LoadRowsSettings settings, bool headerRowEmbedded=true)
        {
            Requires.ZeroRows(dt, nameof(dt));
            Requires.NonNull(rows, nameof(rows));
            settings = settings ?? new LoadRowsSettings();
            if (!((headerRowEmbedded && dt.Columns.Count == 0) || (!headerRowEmbedded && dt.Columns.Count > 0)))
            {
                throw new InvalidOperationException("Header row must be embedded or table must already have columns");
            }

            var e = rows.GetEnumerator();
            if (!e.MoveNext()) return dt;

            bool createColumns = dt.Columns.Count == 0;
            DataColumn rowNumberColumn = null;
            if (settings.RowNumberColumnName != null)
            {
                rowNumberColumn = dt.Columns[settings.RowNumberColumnName];
                if (rowNumberColumn == null)
                {
                    rowNumberColumn = new DataColumn(settings.RowNumberColumnName, typeof(int)) { AllowDBNull = false };
                    dt.Columns.Add(rowNumberColumn);
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
                var columnMapper = settings.ColumnMapper ?? DataTableHelpers.OneToOneColumnNameMapper;
                var duplicateColumnRenamer = settings.DuplicateColumnRenamer ?? DataTableHelpers.OnDuplicateColumnNameThrow;
                columnMap = new DataColumn[headerRow.Count];
                for (int z = 0; z < headerRow.Count(); ++z)
                {
                    var colName = StringHelpers.TrimOrNull(Stuff.ObjectToString(headerRow[z]));
                    if (colName == null)
                    {
                        continue;
                    }
                    colName = columnMapper(colName);
                    if (colName == null)
                    {
                        continue;
                    }
                    var c = dt.Columns[colName];
                    if (createColumns)
                    {
                        if (c == null)
                        {
                            c = new DataColumn(colName);
                            dt.Columns.Add(c);
                        }
                        else
                        {
                            colName = duplicateColumnRenamer(dt, colName);
                            c = new DataColumn(colName);
                            dt.Columns.Add(c);
                        }
                    }
                    else if (c == null)
                    {
                        Trace.WriteLine(string.Format("Will ignore source column #{0} with name=[{1}]", z, colName));
                    }
                    columnMap[z] = c;
                }
                if (!e.MoveNext()) return dt;
            }
            else
            {
                columnMap = dt.Columns.OfType<DataColumn>().ToArray();
            }

            int rowNum = -1;
            var onRowAddError = settings.RowAddErrorHandler ?? RowAddErrorRethrow;
            do
            {
                ++rowNum;
                var row = e.Current;
                if (row.Count == 0) continue;
                var fields = new object[dt.Columns.Count];
                try
                {
                    for (int z = 0; z < columnMap.Length; ++z)
                    {
                        var c = columnMap[z];
                        if (c == null) continue;
                        object val = z >= row.Count ? null : row[z];
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
                    if (null == settings.ShouldAddRow || settings.ShouldAddRow(dt, fields))
                    {
                        dt.Rows.Add(fields);
                    }
                }
                catch (Exception ex)
                {
                    onRowAddError(ex, rowNum);
                }
            }
            while (e.MoveNext());

            return dt;
        }

        public static DataColumn CloneToUnbound(this DataColumn c)
            => new DataColumn(c.ColumnName, c.DataType)
            {
                AllowDBNull = c.AllowDBNull,
                Caption = c.Caption,
                AutoIncrement = c.AutoIncrement,
                AutoIncrementSeed = c.AutoIncrementSeed,
                AutoIncrementStep = c.AutoIncrementStep,
                DateTimeMode = c.DateTimeMode,
                DefaultValue = c.DefaultValue,
                Expression = c.Expression,
                MaxLength = c.MaxLength,
                Namespace = c.Namespace,
                Prefix = c.Prefix,
                ReadOnly = c.ReadOnly,
                Unique = c.Unique
            };

        public static void Append(this DataTable dt, DataTable other, bool appendOtherTableColumns = false)
        {
            Requires.NonNull(other, nameof(other));

            bool sameStructure = dt.Columns.Count == other.Columns.Count;
            var dtWasBlank = dt.Columns.Count == 0;
            var columnNamesToAppend = new List<string>();
            foreach (DataColumn bCol in other.Columns)
            {
                var aCol = dt.Columns[bCol.ColumnName];
                sameStructure = sameStructure && aCol != null && aCol.Ordinal == bCol.Ordinal;
                if (aCol == null)
                {
                    if (!appendOtherTableColumns) continue;
                    dt.Columns.Add(bCol.CloneToUnbound());
                }
                columnNamesToAppend.Add(bCol.ColumnName);
            }
            sameStructure = (sameStructure || dtWasBlank) && (dt.Columns.Count == other.Columns.Count);
            foreach (DataRow bRow in other.Rows)
            {
                if (sameStructure)
                {
                    dt.Rows.Add(bRow.ItemArray);
                }
                else
                {
                    var aRow = dt.NewRow();
                    foreach (var columnName in columnNamesToAppend)
                    {
                        aRow[columnName] = bRow[columnName];
                    }
                    dt.Rows.Add(aRow);
                }
            }
        }

        #region Data Column Helpers

        public static string OneToOneColumnNameMapper(string inboundColumnName)
        {
            return inboundColumnName;
        }

        private static readonly Regex MakeFriendlyExpr = new Regex("[^0-9a-z]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string UpperCamelNoSpecialCharactersColumnNameMapper(string inboundColumnName)
        {
            var s = MakeFriendlyExpr.Replace(inboundColumnName, " ");
            s = s.ToUpperCamelCase();
            return s;
        }

        public static Func<string, string> CreateDictionaryMapper(IDictionary<string, string> m, bool onMissingPassthrough = false)
        {
            Requires.NonNull(m, nameof(m));
            Func<string, string> f = delegate (string s)
            {
                if (m.ContainsKey(s)) return m[s];
                return onMissingPassthrough ? s : null;
            };
            return f;
        }

        public static string OnDuplicateColumnNameThrow(DataTable dt, string duplicateColumnName)
        {
            throw new Exception(string.Format("Datatable cannot have duplicate column names.  [{0}] occurs at least twice", duplicateColumnName));
        }

        public static string OnDuplicateAppendSeqeuntialNumber(DataTable dt, string inboundColumnName)
        {
            for (int z = 2; ; ++z)
            {
                var newName = string.Format("{0}_{1}", inboundColumnName, z);
                if (!dt.Columns.Contains(newName)) return newName;
            }
        }

        #endregion
    }
}
