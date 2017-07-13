using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public static string GenerateCreateTableSQL(this DataTable dt, string schema = "dbo", IDictionary<Type, string> typeMap = null)
        {
            Requires.NonNull(dt, nameof(dt));
            Requires.Text(dt.TableName, nameof(dt.TableName));
            Requires.Text(schema, nameof(schema));

            var sb = new StringBuilder();
            sb.AppendFormat("create table [{0}].[{1}]\n(\n", schema, dt.TableName);
            for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = (DataColumn)dt.Columns[colNum];
                string sqlType;
                if (typeMap != null && typeMap.ContainsKey(dc.DataType))
                {
                    sqlType = typeMap[dc.DataType];
                }
                else if (dc.DataType == typeof(int))
                {
                    sqlType = "int";
                }
                else if (dc.DataType == typeof(bool))
                {
                    sqlType = "bit";
                }
                else if (dc.DataType == typeof(float) ||
                         dc.DataType == typeof(double))
                {
                    sqlType = "float";
                }
                else if (dc.DataType == typeof(DateTime))
                {
                    sqlType = "datetime";
                }
                else if (dc.DataType == typeof(Decimal))
                {
                    sqlType = "money";
                }
                else if (dc.DataType == typeof(Byte))
                {
                    sqlType = "tinyint";
                }
                else if (dc.DataType == typeof(Int16))
                {
                    sqlType = "smallint";
                }
                else if (dc.DataType == typeof(Int64))
                {
                    sqlType = "bigint";
                }
                else if (dc.DataType == typeof(string))
                {
                    sqlType = string.Format("nvarchar({0})",
                        (dc.MaxLength <= 0 || dc.MaxLength > 4000) ? "max" : dc.MaxLength.ToString());
                }
                else
                {
                    throw new ArgumentException(string.Format("cannot translate type {0} to sql", dc.DataType.Name), dc.ColumnName);
                }
                sb.AppendFormat("\t[{0}] {1} {2}{3}\n",
                    dc.ColumnName,
                    sqlType,
                    dc.AllowDBNull ? "NULL" : "NOT NULL",
                    colNum == dt.Columns.Count - 1 ? "" : ",");
            }
            sb.AppendFormat(")\n");
            return sb.ToString();
        }

        public static void IdealizeStringColumns(this DataTable dt, bool trimAndNullifyStringData = false)
        {
            Requires.NonNull(dt, nameof(dt));

            for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = (DataColumn)dt.Columns[colNum];
                Trace.WriteLine($"IdealizeStringColumns table({dt.TableName}) column({dc.ColumnName}) {colNum}/{dt.Columns.Count}");
                if (dc.DataType != typeof(string)) continue;
                var len = 0;
                bool hasNulls = false;
                for (int rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                {
                    var o = dt.Rows[rowNum][dc];
                    var s = o as string;
                    if (s == null)
                    {
                        hasNulls = true;
                    }
                    else
                    {
                        if (trimAndNullifyStringData)
                        {
                            var ts = StringHelpers.TrimOrNull(s);
                            if (ts != s)
                            {
                                if (ts == null)
                                {
                                    hasNulls = true;
                                    dt.Rows[rowNum][dc] = DBNull.Value;
                                    continue;
                                }
                                s = ts;
                                dt.Rows[rowNum][dc] = s;
                            }
                        }
                        len = Math.Max(len, s.Length);
                    }
                }
                dc.AllowDBNull = hasNulls;
                dc.MaxLength = Math.Max(1, len);
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

        public static void RequiresZeroRows(DataTable dt, string argName = null)
        {
            Requires.NonNull(dt, argName ?? nameof(dt));
            if (dt.Rows.Count > 0) throw new ArgumentException("dt must not already have any rows", nameof(dt));
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

        public static void LoadRowsFromDelineatedText(this DataTable dt, Stream st, LoadRowsFromDelineatedTextSettings settings)
        {
            RequiresZeroRows(dt, nameof(dt));
            Requires.ReadableStreamArg(st, nameof(st));
            Requires.NonNull(settings, nameof(settings));

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
            dt.LoadRows(rows.Skip(settings.SkipRawRows), settings);
        }

        public static void LoadRows(this DataTable dt, IEnumerable<IList<object>> rows, LoadRowsSettings settings = null)
        {
            RequiresZeroRows(dt, nameof(dt));
            Requires.NonNull(rows, nameof(rows));
            settings = settings ?? new LoadRowsSettings();

            var e = rows.GetEnumerator();
            if (!e.MoveNext())
            {
                return;
            }

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

            var headerRow = e.Current;
            var columnMapper = settings.ColumnMapper ?? DataTableHelpers.OneToOneColumnNameMapper;
            var duplicateColumnRenamer = settings.DuplicateColumnRenamer ?? DataTableHelpers.OnDuplicateColumnNameThrow;
            var columnMap = new DataColumn[headerRow.Count];
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

            int rowNum = -1;
            var onRowAddError = settings.RowAddErrorHandler ?? RowAddErrorRethrow;
            while (e.MoveNext())
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
