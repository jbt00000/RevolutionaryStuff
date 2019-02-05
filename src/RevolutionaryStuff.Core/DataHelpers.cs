using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Core
{
    public static class DataHelpers
    {
        public static string ConnectionStringAlter(string connectionString, ApplicationIntent intent)
        {
            var csb = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            csb.ApplicationIntent = intent;
            return csb.ConnectionString;
        }

        public static void RowAddErrorRethrow(Exception ex, int rowNum)
        {
            throw new Exception($"Problem adding row {rowNum}", ex);
        }

        public static void RowAddErrorTraceAndIgnore(Exception ex, int rowNum)
        {
            Trace.WriteLine($"Problem adding row {rowNum}.  Will Skip.\n{ex}");
        }

        public static DataTable ToDataTableWithColumnHeaders(this IEnumerable<IList<object>> rows, string name = null, Func<DataTable, string, string> duplicateColumnRenamer = null, Action<Exception, int> onRowAddError = null)
        {
            onRowAddError = onRowAddError ?? RowAddErrorRethrow;
            var dt = new DataTable();
            if (!string.IsNullOrEmpty(name)) dt.TableName = name;
            var headerRow = rows.First();
            var positions = new int?[headerRow.Count];
            var gaps = false;
            for (int z = 0; z < positions.Length; ++z)
            {
                var colName = StringHelpers.TrimOrNull(StringHelpers.ToString(headerRow[z]));
                if (colName == null)
                {
                    gaps = true;
                    continue;
                }
                positions[z] = dt.Columns.Count;
                gaps = gaps || (z > 0 && (positions[z] - 1) > positions[z - 1]);
                if (dt.Columns.Contains(colName) && duplicateColumnRenamer != null)
                {
                    colName = duplicateColumnRenamer(dt, colName);
                }
                dt.Columns.Add(new DataColumn(colName));
            }
            int rowNum = 1;
            foreach (var row in rows.Skip(1))
            {
                ++rowNum;
                object[] fields;
                if (gaps)
                {
                    fields = new object[dt.Columns.Count];
                    for (int z = 0; z < positions.Length; ++z)
                    {
                        var pos = positions[z];
                        if (!pos.HasValue) continue;
                        fields[pos.Value] = row[z];
                    }
                }
                else
                {
                    fields = row.ToArray();
                }
                try
                {
                    dt.Rows.Add(fields);
                }
                catch (Exception ex)
                {
                    onRowAddError(ex, rowNum);
                }
            }
            return dt;
        }

        public static void RightType(this DataTable dt, bool nullifyBlankStrings=true)
        {
            Requires.NonNull(dt, nameof(dt));
            using (new TraceRegion($"{nameof(RightType)} table({dt.TableName}) with {dt.Columns.Count} columns and {dt.Rows.Count} rows"))
            {
                var columnNames = new List<string>();
                for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
                {
                    var dc = dt.Columns[colNum];
                    columnNames.Add(dc.ColumnName);
                    dc.SetOrdinal(colNum);
                }
                for (int colNum = 0; colNum < columnNames.Count; ++colNum)
                {
                    var dc = dt.Columns[columnNames[colNum]];
                    if (dc.DataType != typeof(string)) continue;
                    if (dc.PreserveTypeInformation()) continue;
                    dc.AllowDBNull = true;
                    Trace.WriteLine($"{nameof(RightType)} table({dt.TableName}) column({dc.ColumnName}) {colNum}/{dt.Columns.Count}");
                    var len = 0;
                    bool hasNulls = false;
                    bool hasLeadingZeros = false;
                    bool canBeVarchar = true;
                    bool canBeDate = true;
                    bool canBeDatetime = true;
                    bool canBeBit = true;
                    bool canBeYN = true;
                    bool canBeYesNo = true;
                    bool canBeTrueFalse = true;
                    bool canBeInt64 = true;
                    bool canBeInt32 = true;
                    bool canBeInt16 = true;
                    bool canBeInt8 = true;
                    bool canBeFloat = true;
                    bool canBeDecimal = true;
                    bool canBeDateTimeOffset = true;
                    for (int rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                    {
                        var dr = dt.Rows[rowNum];
                        var s = dr[colNum] as string;
                        if (s == null)
                        {
                            hasNulls = true;
                            continue;
                        }
                        string zs = s.Trim();
                        if (nullifyBlankStrings && zs == "")
                        {
                            hasNulls = true;
                            dr[colNum] = DBNull.Value;
                            continue;
                        }
                        canBeVarchar = canBeVarchar && zs.ContainsOnlyExtendedAsciiCharacters();
                        if (canBeDate || canBeDatetime)
                        {
                            if (DateTime.TryParse(zs, out var d))
                            {
                                canBeDate = canBeDate && d.Hour == 0 && d.Minute == 0 && d.Second == 0;
                            }
                            else
                            {
                                canBeDate = canBeDatetime = false;
                            }
                        }
                        canBeBit = canBeBit && (zs == "1" || zs == "0");
                        canBeYN = canBeYN && (0 == string.Compare(zs, "y", true) || 0 == string.Compare(zs, "n", true));
                        canBeYesNo = canBeYesNo && (0 == string.Compare(zs, "yes", true) || 0 == string.Compare(zs, "no", true));
                        canBeTrueFalse = canBeTrueFalse && (0 == string.Compare(zs, "true", true) || 0 == string.Compare(zs, "false", true));
                        canBeInt64 = canBeInt64 && Int64.TryParse(zs, out var int64Test);
                        canBeInt32 = canBeInt32 && Int32.TryParse(zs, out var int32Test);
                        canBeInt16 = canBeInt16 && Int16.TryParse(zs, out var int16Test);
                        canBeInt8 = canBeInt8 && Byte.TryParse(zs, out var int8Test);
                        canBeFloat = canBeFloat && double.TryParse(zs, out var doubleTest);
                        canBeDecimal = canBeDecimal && Decimal.TryParse(zs, out var decimalTest);
                        canBeDateTimeOffset = canBeDateTimeOffset && DateTimeOffset.TryParse(zs, out var dateTimeOffsetTest);
                        hasLeadingZeros = hasLeadingZeros || (zs.Length > 1 && zs[0] == '0' && zs[1] != '.');
                        if (zs != s)
                        {
                            dr[colNum] = zs;
                        }
                        len = Stuff.Max(len, zs.Length);
                    }
                    Func<string, object> converter = null;
                    Type convertedDataType = typeof(object);
                    if (canBeBit)
                    {
                        converter = q => q == "1";
                        convertedDataType = typeof(bool);
                    }
                    else if (canBeYN)
                    {
                        converter = q => string.Compare(q, "y", true);
                        convertedDataType = typeof(bool);
                    }
                    else if (canBeYesNo)
                    {
                        converter = q => string.Compare(q, "yes", true);
                        convertedDataType = typeof(bool);
                    }
                    else if (canBeTrueFalse)
                    {
                        converter = q => string.Compare(q, "true", true);
                        convertedDataType = typeof(bool);
                    }
                    else if (canBeInt8 && !hasLeadingZeros)
                    {
                        converter = q => Byte.Parse(q);
                        convertedDataType = typeof(Byte);
                    }
                    else if (canBeInt16 && !hasLeadingZeros)
                    {
                        converter = q => Int16.Parse(q);
                        convertedDataType = typeof(Int16);
                    }
                    else if (canBeInt32 && !hasLeadingZeros)
                    {
                        converter = q => Int32.Parse(q);
                        convertedDataType = typeof(Int32);
                    }
                    else if (canBeInt64 && !hasLeadingZeros)
                    {
                        converter = q => Int64.Parse(q);
                        convertedDataType = typeof(Int64);
                    }
                    else if (canBeFloat && !hasLeadingZeros)
                    {
                        converter = q => Double.Parse(q);
                        convertedDataType = typeof(Double);
                    }
                    else if (canBeDecimal && !hasLeadingZeros)
                    {
                        converter = q => Decimal.Parse(q);
                        convertedDataType = typeof(Decimal);
                    }
                    else if (canBeDateTimeOffset)
                    {
                        converter = q => DateTimeOffset.Parse(q);
                        convertedDataType = typeof(DateTimeOffset);
                    }
                    else if (canBeDate)
                    {
                        converter = q => DateTime.Parse(q);
                        convertedDataType = typeof(DateTime);
                    }
                    else if (canBeDatetime)
                    {
                        converter = q => DateTime.Parse(q);
                        convertedDataType = typeof(DateTime);
                    }
                    else if (canBeVarchar)
                    {
                        dc.Unicode(false);
                        dc.MaxLength = Math.Max(1, len);
                    }
                    else
                    {
                        dc.MaxLength = Math.Max(1, len);
                    }
                    dc.AllowDBNull = hasNulls;
                    if (converter != null)
                    {
                        Trace.WriteLine($"Converting {dc.ColumnName} to {convertedDataType}");
                        var colName = dc.ColumnName;
                        dc.ColumnName = "___DEAD___" + dc.ColumnName;
                        var newCol = new DataColumn(colName, convertedDataType) { AllowDBNull = dc.AllowDBNull };
                        dt.Columns.Add(newCol);
                        var convertedColNum = dt.Columns.IndexOf(colName);
                        string lastStr = null;
                        object lastConv = null;
                        for (int rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                        {
                            var dr = dt.Rows[rowNum];
                            var s = dr[colNum] as string;
                            if (s == null) continue;
                            if (s == lastStr)
                            {
                                dr[convertedColNum] = lastConv;
                            }
                            else
                            {
                                dr[convertedColNum] = lastConv = converter(s);
                                lastStr = s;
                            }
                        }
                        dt.Columns.Remove(dc);
                        newCol.SetOrdinal(colNum);
                    }
                }
            }
        }

        public static void IdealizeStringColumns(this DataTable dt, bool trimAndNullifyStringData = false)
        {
            Requires.NonNull(dt, nameof(dt));
            using (new TraceRegion($"{nameof(IdealizeStringColumns)} table({dt.TableName}) with {dt.Columns.Count} columns and {dt.Rows.Count} rows"))
            {
                for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
                {
                    var dc = dt.Columns[colNum];
                    if (dc.DataType != typeof(string)) continue;
                    dc.AllowDBNull = true;
                    Trace.WriteLine($"{nameof(IdealizeStringColumns)} table({dt.TableName}) column({dc.ColumnName}) {colNum}/{dt.Columns.Count}");
                    var len = 0;
                    bool hasNulls = false;
                    for (int rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                    {
                        var dr = dt.Rows[rowNum];
                        var s = dr[colNum] as string;
                        if (s == null)
                        {
                            hasNulls = true;
                        }
                        else if (trimAndNullifyStringData)
                        {
                            var ts = StringHelpers.TrimOrNull(s);
                            if (ts != s)
                            {
                                if (ts == null)
                                {
                                    hasNulls = true;
                                    dr[colNum] = DBNull.Value;
                                    continue;
                                }
                                else
                                {
                                    s = ts;
                                    dr[colNum] = s;
                                }
                            }
                            len = Stuff.Max(len, s.Length);
                        }
                    }
                    dc.AllowDBNull = hasNulls;
                    dc.MaxLength = Math.Max(1, len);
                }
            }
        }

        public static string GenerateCreateTableSQL(this DataTable dt, string schema = null, IDictionary<Type, string> typeMap = null, string extraColumnSql = null, string autoNumberColumnName=null)
        {
            schema = schema ?? "dbo";
            var sb = new StringBuilder();
            sb.AppendFormat("create table [{0}].[{1}]\n(\n", schema, dt.TableName);
            if (autoNumberColumnName != null)
            {
                sb.AppendLine($"\t{autoNumberColumnName} int not null identity primary key,\n");
            }
            for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = dt.Columns[colNum];
                string sqlType;
                if (typeMap != null && typeMap.ContainsKey(dc.DataType))
                {
                    sqlType = typeMap[dc.DataType];
                }
                else if (dc.DataType == typeof(Int64))
                {
                    sqlType = "bigint";
                }
                else if (dc.DataType == typeof(Int32))
                {
                    sqlType = "int";
                }
                else if (dc.DataType == typeof(Int16))
                {
                    sqlType = "smallint";
                }
                else if (dc.DataType == typeof(Byte))
                {
                    sqlType = "tinyint";
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
                else if (dc.DataType == typeof(Decimal))
                {
                    sqlType = "decimal";
                    int precision = dc.NumericPrecision();
                    int scale = dc.NumericScale();
                    if (scale > 0 && precision > 0)
                    {
                        sqlType += $"({precision},{scale})";
                    }
                }
                else if (dc.DataType == typeof(DateTime))
                {
                    sqlType = "datetime";
                }
                else if (dc.DataType == typeof(DateTimeOffset))
                {
                    sqlType = "datetimeoffset";
                }
                else if (dc.DataType == typeof(string))
                {
                    sqlType = string.Format(
                        "{0}({1})",
                        dc.Unicode() ? "nvarchar" : "varchar",
                        (dc.MaxLength <= 0 || dc.MaxLength > 4000) ? "max" : dc.MaxLength.ToString());
                }
                else
                {
                    throw new ArgumentException(string.Format("cannot translate type {0} to sql", dc.DataType.Name), dc.ColumnName);
                }
                var isPk = dt.PrimaryKey != null && dt.PrimaryKey.Length == 1 && dt.PrimaryKey[0] == dc;
                sb.AppendFormat("\t[{0}] {1} {2}{3}{4}\n",
                    dc.ColumnName,
                    sqlType,
                    dc.AllowDBNull ? "NULL" : "NOT NULL",
                    isPk ? " PRIMARY KEY" : "",
                    colNum == dt.Columns.Count - 1 ? "" : ",");
            }
            if (extraColumnSql != null)
            {
                sb.Append(","+extraColumnSql);
            }
            sb.AppendFormat(")\n");
            return sb.ToString();
        }

        public static int GetIdentityAsInt(this IDataReader r, int col)
        {
            return Convert.ToInt32(r.GetValue(col));
        }

        public static string GetNullableString(this IDataReader r, int col)
        {
            if (r.IsDBNull(col)) return null;
            return r.GetString(col);
        }

        public static DateTime? GetNullableDateTime(this IDataReader r, int col)
        {
            if (r.IsDBNull(col)) return null;
            return r.GetDateTime(col);
        }

        public static Int32? GetNullableInt32(this IDataReader r, int col)
        {
            if (r.IsDBNull(col)) return null;
            return r.GetInt32(col);
        }

        public static bool GetBool(this IDataReader r, int col, bool fallback)
        {
            if (r.IsDBNull(col)) return fallback;
            return r.GetBoolean(col);
        }

        public static bool? GetNullableBool(this IDataReader r, int col)
        {
            if (r.IsDBNull(col)) return null;
            return r.GetBoolean(col);
        }
    }
}
