using System.Data;
using System.Diagnostics;
using System.Text;
using Microsoft.Data.SqlClient;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Core;

public static class DataHelpers
{
    public static string ConnectionStringAlter(string connectionString, ApplicationIntent intent)
    {
        var csb = new SqlConnectionStringBuilder(connectionString);
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
        onRowAddError ??= RowAddErrorRethrow;
        var dt = new DataTable();
        if (!string.IsNullOrEmpty(name)) dt.TableName = name;
        var headerRow = rows.First();
        var positions = new int?[headerRow.Count];
        var gaps = false;
        for (var z = 0; z < positions.Length; ++z)
        {
            var colName = StringHelpers.ToString(headerRow[z]).TrimOrNull();
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
        var rowNum = 1;
        foreach (var row in rows.Skip(1))
        {
            ++rowNum;
            object[] fields;
            if (gaps)
            {
                fields = new object[dt.Columns.Count];
                for (var z = 0; z < positions.Length; ++z)
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

    private class RightTypeInfo
    {
        public readonly DataColumn SourceColumn;
        public Func<string, object> Converter;
        public Type DataType;
        public int? MaxLength;
        public bool AllowNull;
        public bool? Unicode;
        public bool AsIs;

        public RightTypeInfo(DataColumn sourceColumn)
        {
            SourceColumn = sourceColumn;
        }

        public override string ToString()
            => $"{GetType().Name} colName=[{SourceColumn.ColumnName}] type={DataType.Name} maxLength={MaxLength} nullable={AllowNull} unicode={Unicode} ";

        public DataColumn CreateDataColumn()
        {
            var c = SourceColumn.Xerox();
            if (!AsIs)
            {
                c.DataType = DataType;
                if (MaxLength.HasValue)
                {
                    c.MaxLength = MaxLength.Value;
                }
                c.AllowDBNull = AllowNull;
                if (c.DataType == typeof(string))
                {
                    c.Unicode(Unicode);
                }
            }
            return c;
        }
    }

    public static DataTable RightType(this DataTable dt, bool nullifyBlankStrings = true, bool returnNewTable = false, Predicate<DataColumn> columnFilter = null)
    {
        Requires.NonNull(dt, nameof(dt));
        if (columnFilter == null)
        {
            columnFilter = _ => true;
        }
        else
        {
            Requires.False(returnNewTable, nameof(returnNewTable));
        }

        using (new TraceRegion($"{nameof(RightType)} table({dt.TableName}) with {dt.Columns.Count} columns and {dt.Rows.Count} rows"))
        {
            var rtis = new List<RightTypeInfo>();
            var columnNames = new List<string>();
            for (var colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = dt.Columns[colNum];
                columnNames.Add(dc.ColumnName);
                dc.SetOrdinal(colNum);
            }
            for (var colNum = 0; colNum < columnNames.Count; ++colNum)
            {
                var dc = dt.Columns[columnNames[colNum]];
                //var dc = dt.Columns[colNum];
                if (dc.DataType != typeof(string) || dc.PreserveTypeInformation() || !columnFilter(dc))
                {
                    rtis.Add(new RightTypeInfo(dc) { AsIs = true });
                    continue;
                }
                dc.AllowDBNull = true;
                Trace.WriteLine($"{nameof(RightType)} table({dt.TableName}) column({dc.ColumnName}) {colNum}/{dt.Columns.Count}");
                var len = 0;
                var hasNulls = false;
                var hasLeadingZeros = false;
                var canBeVarchar = true;
                var canBeDate = true;
                var canBeDatetime = true;
                var canBeBit = true;
                var canBeYN = true;
                var canBeYesNo = true;
                var canBeTrueFalse = true;
                var canBeInt64 = true;
                var canBeInt32 = true;
                var canBeInt16 = true;
                var canBeInt8 = true;
                var canBeFloat = true;
                var canBeIntFromFloat = true;
                var canBeDecimal = true;
                var canBeDateTimeOffset = true;
                for (var rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                {
                    var dr = dt.Rows[rowNum];
                    var s = dr[colNum] as string;
                    if (s == null)
                    {
                        hasNulls = true;
                        continue;
                    }
                    var zs = s.Trim();
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
                    canBeBit = canBeBit && zs is "1" or "0";
                    canBeYN = canBeYN && (0 == string.Compare(zs, "y", true) || 0 == string.Compare(zs, "n", true));
                    canBeYesNo = canBeYesNo && (0 == string.Compare(zs, "yes", true) || 0 == string.Compare(zs, "no", true));
                    canBeTrueFalse = canBeTrueFalse && (0 == string.Compare(zs, "true", true) || 0 == string.Compare(zs, "false", true));
                    canBeInt64 = canBeInt64 && Int64.TryParse(zs, out var int64Test);
                    canBeInt32 = canBeInt32 && Int32.TryParse(zs, out var int32Test);
                    canBeInt16 = canBeInt16 && Int16.TryParse(zs, out var int16Test);
                    canBeInt8 = canBeInt8 && Byte.TryParse(zs, out var int8Test);
                    var doubleTest = 0.1;
                    canBeFloat = canBeFloat && double.TryParse(zs, out doubleTest);
                    canBeIntFromFloat = canBeIntFromFloat && canBeFloat && doubleTest == Math.Truncate(doubleTest) && doubleTest <= int.MaxValue && doubleTest >= int.MinValue;
                    canBeDecimal = canBeDecimal && Decimal.TryParse(zs, out var decimalTest);
                    canBeDateTimeOffset = canBeDateTimeOffset && DateTimeOffset.TryParse(zs, out var dateTimeOffsetTest);
                    hasLeadingZeros = hasLeadingZeros || (zs.Length > 1 && zs[0] == '0' && zs[1] != '.');
                    if (zs != s)
                    {
                        dr[colNum] = zs;
                    }
                    len = Stuff.Max(len, zs.Length);
                }
                var rti = new RightTypeInfo(dc)
                {
                    AllowNull = hasNulls,
                    DataType = typeof(object)
                };
                rtis.Add(rti);
                if (canBeBit)
                {
                    rti.Converter = q => q == "1";
                    rti.DataType = typeof(bool);
                }
                else if (canBeYN)
                {
                    rti.Converter = q => string.Compare(q, "y", true) == 0;
                    rti.DataType = typeof(bool);
                }
                else if (canBeYesNo)
                {
                    rti.Converter = q => string.Compare(q, "yes", true) == 0;
                    rti.DataType = typeof(bool);
                }
                else if (canBeTrueFalse)
                {
                    rti.Converter = q => string.Compare(q, "true", true) == 0;
                    rti.DataType = typeof(bool);
                }
                else if (canBeInt8 && !hasLeadingZeros)
                {
                    rti.Converter = q => (byte)Convert.ToDouble(q);
                    rti.DataType = typeof(Byte);
                }
                else if (canBeInt16 && !hasLeadingZeros)
                {
                    rti.Converter = q => (Int16)Convert.ToDouble(q);
                    rti.DataType = typeof(Int16);
                }
                else if (canBeInt32 && !hasLeadingZeros)
                {
                    rti.Converter = q => (Int32)Convert.ToDouble(q);
                    rti.DataType = typeof(Int32);
                }
                else if (canBeInt64 && !hasLeadingZeros)
                {
                    rti.Converter = q => (Int64)Convert.ToDouble(q);
                    rti.DataType = typeof(Int64);
                }
                else if (canBeIntFromFloat && !hasLeadingZeros)
                {
                    rti.Converter = q => (int)Math.Truncate(Double.Parse(q));
                    rti.DataType = typeof(Int32);
                }
                else if (canBeFloat && !hasLeadingZeros)
                {
                    rti.Converter = q => Double.Parse(q);
                    rti.DataType = typeof(Double);
                }
                else if (canBeDecimal && !hasLeadingZeros)
                {
                    rti.Converter = q => Decimal.Parse(q);
                    rti.DataType = typeof(Decimal);
                }
                else if (canBeDateTimeOffset)
                {
                    rti.Converter = q => DateTimeOffset.Parse(q);
                    rti.DataType = typeof(DateTimeOffset);
                }
                else if (canBeDate)
                {
                    rti.Converter = q => DateTime.Parse(q);
                    rti.DataType = typeof(DateTime);
                }
                else if (canBeDatetime)
                {
                    rti.Converter = q => DateTime.Parse(q);
                    rti.DataType = typeof(DateTime);
                }
                else if (canBeVarchar)
                {
                    rti.Unicode = false;
                    rti.MaxLength = Math.Max(1, len);
                    rti.DataType = typeof(string);
                    if (!returnNewTable)
                    {
                        dc.Unicode(false);
                        dc.MaxLength = Math.Max(1, len);
                    }
                }
                else
                {
                    rti.Unicode = true;
                    rti.MaxLength = Math.Max(1, len);
                    rti.DataType = typeof(string);
                    if (!returnNewTable)
                    {
                        dc.MaxLength = Math.Max(1, len);
                    }
                }
                if (returnNewTable)
                {
                    Trace.WriteLine($"Will convert col[{colNum}=`{dc.ColumnName}`] via {rti}");
                }
                else
                {
                    dc.AllowDBNull = rti.AllowNull;
                    if (rti.Converter != null)
                    {
                        Trace.WriteLine($"Converting {dc.ColumnName} to {rti.DataType}");
                        var colName = dc.ColumnName;
                        dc.ColumnName = "___DEAD___" + dc.ColumnName;
                        var newCol = new DataColumn(colName, rti.DataType) { AllowDBNull = dc.AllowDBNull };
                        dt.Columns.Add(newCol);
                        var convertedColNum = dt.Columns.IndexOf(colName);
                        string lastStr = null;
                        object lastConv = null;
                        for (var rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
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
                                dr[convertedColNum] = lastConv = rti.Converter(s);
                                lastStr = s;
                            }
                        }
                        dt.Columns.Remove(dc);
                        newCol.SetOrdinal(colNum);
                    }
                }
            }
            if (returnNewTable)
            {
                using (new TraceRegion($"converting into new datatable"))
                {
                    var sdt = dt;
                    dt = new DataTable(sdt.TableName, sdt.Namespace);
                    foreach (var rti in rtis)
                    {
                        var c = rti.CreateDataColumn();
                        dt.Columns.Add(c);
                    }
                    var rowCount = sdt.Rows.Count;
                    while (sdt.Rows.Count > 0)
                    {
                        var srow = sdt.Rows[0];
                        var drow = dt.NewRow();
                        for (var z = 0; z < rtis.Count; ++z)
                        {
                            var rti = rtis[z];
                            var v = srow[z];
                            if (v == DBNull.Value)
                            {
                                drow[z] = DBNull.Value;
                            }
                            else if (rti.Converter == null || !(v is string))
                            {
                                drow[z] = v;
                            }
                            else
                            {
                                v = rti.Converter(v as string);
                                if (v == null)
                                {
                                    drow[z] = DBNull.Value;
                                }
                                else
                                {
                                    drow[z] = v;
                                }
                            }
                        }
                        dt.Rows.Add(drow);
                        sdt.Rows.RemoveAt(0);
                        if (dt.Rows.Count % 100000 == 0)
                        {
                            Trace.WriteLine($"converted {dt.Rows.Count}/{rowCount} rows");
                        }
                    }
                }
            }
        }
        return dt;
    }

    public static void IdealizeStringColumns(this DataTable dt, bool trimAndNullifyStringData = false)
    {
        Requires.NonNull(dt, nameof(dt));
        using (new TraceRegion($"{nameof(IdealizeStringColumns)} table({dt.TableName}) with {dt.Columns.Count} columns and {dt.Rows.Count} rows"))
        {
            for (var colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = dt.Columns[colNum];
                if (dc.DataType != typeof(string)) continue;
                dc.AllowDBNull = true;
                Trace.WriteLine($"{nameof(IdealizeStringColumns)} table({dt.TableName}) column({dc.ColumnName}) {colNum}/{dt.Columns.Count}");
                var len = 0;
                var hasNulls = false;
                for (var rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                {
                    var dr = dt.Rows[rowNum];
                    var s = dr[colNum] as string;
                    if (s == null)
                    {
                        hasNulls = true;
                    }
                    else
                    {
                        if (trimAndNullifyStringData)
                        {
                            var ts = s.TrimOrNull();
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
                        }
                        len = Stuff.Max(len, s.Length);
                    }
                }
                dc.AllowDBNull = hasNulls;
                dc.MaxLength = Math.Max(1, len);
            }
        }
    }

    public static string GenerateCreateTableSQL(this DataTable dt, string schema = null, IDictionary<Type, string> typeMap = null, string extraColumnSql = null, string autoNumberColumnName = null)
    {
        schema ??= "dbo";
        var sb = new StringBuilder();
        sb.AppendFormat("create table [{0}].[{1}]\n(\n", schema, dt.TableName);
        if (autoNumberColumnName != null)
        {
            sb.AppendLine($"\t{autoNumberColumnName} int not null identity primary key,\n");
        }
        for (var colNum = 0; colNum < dt.Columns.Count; ++colNum)
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
                var precision = dc.NumericPrecision();
                var scale = dc.NumericScale();
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
                    dc.MaxLength is <= 0 or > 4000 ? "max" : dc.MaxLength.ToString());
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
            sb.Append("," + extraColumnSql);
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
