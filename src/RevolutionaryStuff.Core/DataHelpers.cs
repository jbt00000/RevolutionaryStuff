using System.Data;
using System.Diagnostics;
using System.Text;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Core;

public static class DataHelpers
{
    public static void RowAddErrorRethrow(Exception ex, int rowNum)
    {
        throw new Exception($"Problem adding row {rowNum}", ex);
    }

    public static void RowAddErrorTraceAndIgnore(Exception ex, int rowNum)
    {
        Trace.WriteLine($"Problem adding row {rowNum}.  Will Skip.\n{ex}");
    }

    extension(IEnumerable<IList<object>> sources)
    {
        public DataTable ToDataTableWithColumnHeaders(string name = null, Func<DataTable, string, string> duplicateColumnRenamer = null, Action<Exception, int> onRowAddError = null)
        {
            onRowAddError ??= RowAddErrorRethrow;
            var dt = new DataTable();
            if (!string.IsNullOrEmpty(name)) dt.TableName = name;
            var headerRow = sources.First();
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
            foreach (var row in sources.Skip(1))
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

    extension(DataTable source)
    {
        public DataTable RightType(bool nullifyBlankStrings = true, bool returnNewTable = false, Predicate<DataColumn> columnFilter = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (columnFilter == null)
            {
                columnFilter = _ => true;
            }
            else
            {
                Requires.False(returnNewTable);
            }

            using (new TraceRegion($"{nameof(RightType)} table({source.TableName}) with {source.Columns.Count} columns and {source.Rows.Count} rows"))
            {
                var rtis = new List<RightTypeInfo>();
                var columnNames = new List<string>();
                for (var colNum = 0; colNum < source.Columns.Count; ++colNum)
                {
                    var dc = source.Columns[colNum];
                    columnNames.Add(dc.ColumnName);
                    dc.SetOrdinal(colNum);
                }
                for (var colNum = 0; colNum < columnNames.Count; ++colNum)
                {
                    var dc = source.Columns[columnNames[colNum]];
                    //var dc = dt.Columns[colNum];
                    if (dc.DataType != typeof(string) || dc.PreserveTypeInformation() || !columnFilter(dc))
                    {
                        rtis.Add(new RightTypeInfo(dc) { AsIs = true });
                        continue;
                    }
                    dc.AllowDBNull = true;
                    Trace.WriteLine($"{nameof(RightType)} table({source.TableName}) column({dc.ColumnName}) {colNum}/{source.Columns.Count}");
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
                    for (var rowNum = 0; rowNum < source.Rows.Count; ++rowNum)
                    {
                        var dr = source.Rows[rowNum];
                        if (dr[colNum] is not string s)
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
                            canBeDate = DateTime.TryParse(zs, out var d) ? canBeDate && d.Hour == 0 && d.Minute == 0 && d.Second == 0 : (canBeDatetime = false);
                        }
                        canBeBit = canBeBit && zs is "1" or "0";
                        canBeYN = canBeYN && (0 == string.Compare(zs, "y", true) || 0 == string.Compare(zs, "n", true));
                        canBeYesNo = canBeYesNo && (0 == string.Compare(zs, "yes", true) || 0 == string.Compare(zs, "no", true));
                        canBeTrueFalse = canBeTrueFalse && (0 == string.Compare(zs, "true", true) || 0 == string.Compare(zs, "false", true));
                        canBeInt64 = canBeInt64 && long.TryParse(zs, out var int64Test);
                        canBeInt32 = canBeInt32 && int.TryParse(zs, out var int32Test);
                        canBeInt16 = canBeInt16 && short.TryParse(zs, out var int16Test);
                        canBeInt8 = canBeInt8 && byte.TryParse(zs, out var int8Test);
                        var doubleTest = 0.1;
                        canBeFloat = canBeFloat && double.TryParse(zs, out doubleTest);
                        canBeIntFromFloat = canBeIntFromFloat && canBeFloat && doubleTest == Math.Truncate(doubleTest) && doubleTest <= int.MaxValue && doubleTest >= int.MinValue;
                        canBeDecimal = canBeDecimal && decimal.TryParse(zs, out var decimalTest);
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
                        rti.DataType = typeof(byte);
                    }
                    else if (canBeInt16 && !hasLeadingZeros)
                    {
                        rti.Converter = q => (short)Convert.ToDouble(q);
                        rti.DataType = typeof(short);
                    }
                    else if (canBeInt32 && !hasLeadingZeros)
                    {
                        rti.Converter = q => (int)Convert.ToDouble(q);
                        rti.DataType = typeof(int);
                    }
                    else if (canBeInt64 && !hasLeadingZeros)
                    {
                        rti.Converter = q => (long)Convert.ToDouble(q);
                        rti.DataType = typeof(long);
                    }
                    else if (canBeIntFromFloat && !hasLeadingZeros)
                    {
                        rti.Converter = q => (int)Math.Truncate(double.Parse(q));
                        rti.DataType = typeof(int);
                    }
                    else if (canBeFloat && !hasLeadingZeros)
                    {
                        rti.Converter = q => double.Parse(q);
                        rti.DataType = typeof(double);
                    }
                    else if (canBeDecimal && !hasLeadingZeros)
                    {
                        rti.Converter = q => decimal.Parse(q);
                        rti.DataType = typeof(decimal);
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
                            source.Columns.Add(newCol);
                            var convertedColNum = source.Columns.IndexOf(colName);
                            string lastStr = null;
                            object lastConv = null;
                            for (var rowNum = 0; rowNum < source.Rows.Count; ++rowNum)
                            {
                                var dr = source.Rows[rowNum];
                                if (dr[colNum] is not string s) continue;
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
                            source.Columns.Remove(dc);
                            newCol.SetOrdinal(colNum);
                        }
                    }
                }
                if (returnNewTable)
                {
                    using (new TraceRegion($"converting into new datatable"))
                    {
                        var sdt = source;
                        source = new DataTable(sdt.TableName, sdt.Namespace);
                        foreach (var rti in rtis)
                        {
                            var c = rti.CreateDataColumn();
                            source.Columns.Add(c);
                        }
                        var rowCount = sdt.Rows.Count;
                        while (sdt.Rows.Count > 0)
                        {
                            var srow = sdt.Rows[0];
                            var drow = source.NewRow();
                            for (var z = 0; z < rtis.Count; ++z)
                            {
                                var rti = rtis[z];
                                var v = srow[z];
                                if (v == DBNull.Value)
                                {
                                    drow[z] = DBNull.Value;
                                }
                                else if (rti.Converter == null || v is not string)
                                {
                                    drow[z] = v;
                                }
                                else
                                {
                                    v = rti.Converter(v as string);
                                    drow[z] = v ?? DBNull.Value;
                                }
                            }
                            source.Rows.Add(drow);
                            sdt.Rows.RemoveAt(0);
                            if (source.Rows.Count % 100000 == 0)
                            {
                                Trace.WriteLine($"converted {source.Rows.Count}/{rowCount} rows");
                            }
                        }
                    }
                }
            }
            return source;
        }
        public void IdealizeStringColumns(bool trimAndNullifyStringData = false)
        {
            ArgumentNullException.ThrowIfNull(source);
            using (new TraceRegion($"{nameof(IdealizeStringColumns)} table({source.TableName}) with {source.Columns.Count} columns and {source.Rows.Count} rows"))
            {
                for (var colNum = 0; colNum < source.Columns.Count; ++colNum)
                {
                    var dc = source.Columns[colNum];
                    if (dc.DataType != typeof(string)) continue;
                    dc.AllowDBNull = true;
                    Trace.WriteLine($"{nameof(IdealizeStringColumns)} table({source.TableName}) column({dc.ColumnName}) {colNum}/{source.Columns.Count}");
                    var len = 0;
                    var hasNulls = false;
                    for (var rowNum = 0; rowNum < source.Rows.Count; ++rowNum)
                    {
                        var dr = source.Rows[rowNum];
                        if (dr[colNum] is not string s)
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
        public string GenerateCreateTableSQL(string schema = null, IDictionary<Type, string> typeMap = null, string extraColumnSql = null, string autoNumberColumnName = null)
        {
            schema ??= "dbo";
            var sb = new StringBuilder();
            sb.AppendFormat("create table [{0}].[{1}]\n(\n", schema, source.TableName);
            if (autoNumberColumnName != null)
            {
                sb.AppendLine($"\t{autoNumberColumnName} int not null identity primary key,\n");
            }
            for (var colNum = 0; colNum < source.Columns.Count; ++colNum)
            {
                var dc = source.Columns[colNum];
                string sqlType;
                if (typeMap != null && typeMap.ContainsKey(dc.DataType))
                {
                    sqlType = typeMap[dc.DataType];
                }
                else if (dc.DataType == typeof(long))
                {
                    sqlType = "bigint";
                }
                else if (dc.DataType == typeof(int))
                {
                    sqlType = "int";
                }
                else if (dc.DataType == typeof(short))
                {
                    sqlType = "smallint";
                }
                else if (dc.DataType == typeof(byte))
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
                else if (dc.DataType == typeof(decimal))
                {
                    sqlType = "decimal";
                    var precision = dc.NumericPrecision();
                    var scale = dc.NumericScale();
                    if (scale > 0 && precision > 0)
                    {
                        sqlType += $"({precision},{scale})";
                    }
                }
                else
                {
                    sqlType = dc.DataType == typeof(DateTime)
                        ? "datetime"
                        : dc.DataType == typeof(DateTimeOffset)
                                        ? "datetimeoffset"
                                        : dc.DataType == typeof(string)
                                                        ? string.Format(
                                                                        "{0}({1})",
                                                                        dc.Unicode() ? "nvarchar" : "varchar",
                                                                        dc.MaxLength is <= 0 or > 4000 ? "max" : dc.MaxLength.ToString())
                                                        : throw new ArgumentException($"cannot translate type {dc.DataType.Name} to sql", dc.ColumnName);
                }
                var isPk = source.PrimaryKey != null && source.PrimaryKey.Length == 1 && source.PrimaryKey[0] == dc;
                sb.AppendFormat("\t[{0}] {1} {2}{3}{4}\n",
                    dc.ColumnName,
                    sqlType,
                    dc.AllowDBNull ? "NULL" : "NOT NULL",
                    isPk ? " PRIMARY KEY" : "",
                    colNum == source.Columns.Count - 1 ? "" : ",");
            }
            if (extraColumnSql != null)
            {
                sb.Append("," + extraColumnSql);
            }
            sb.AppendFormat(")\n");
            return sb.ToString();
        }
    }





    extension(IDataReader source)
    {
        public int GetIdentityAsInt(int col)
        {
            return Convert.ToInt32(source.GetValue(col));
        }
        public string GetNullableString(int col)
        {
            return source.IsDBNull(col) ? null : source.GetString(col);
        }
        public DateTime? GetNullableDateTime(int col)
        {
            return source.IsDBNull(col) ? null : source.GetDateTime(col);
        }
        public int? GetNullableInt32(int col)
        {
            return source.IsDBNull(col) ? null : source.GetInt32(col);
        }
        public bool GetBool(int col, bool fallback)
        {
            return source.IsDBNull(col) ? fallback : source.GetBoolean(col);
        }
        public bool? GetNullableBool(int col)
        {
            return source.IsDBNull(col) ? null : source.GetBoolean(col);
        }
    }










}
