using RevolutionaryStuff.Core.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

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

        public static IDataTable ToDataTableWithColumnHeaders(this IEnumerable<IList<object>> rows, string name = null, Func<IDataTable, string, string> duplicateColumnRenamer = null, Action<Exception, int> onRowAddError = null)
        {
            onRowAddError = onRowAddError ?? RowAddErrorRethrow;
            var dt = new SimpleDataTable();
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
                dt.Columns.Add(new SimpleDataColumn(colName));
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

        public static void IdealizeStringColumns(this IDataTable dt, bool trimAndNullifyStringData = false)
        {
            Requires.NonNull(dt, nameof(dt));
            for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = (IDataColumn)dt.Columns[colNum];
                if (dc.DataType != typeof(string)) continue;
                var len = 0;
                bool hasNulls = false;
                for (int rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                {
                    var dr = dt.Rows[rowNum];
                    var o = dr[dc.ColumnName];
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
                                    dr[dc.ColumnName] = DBNull.Value;
                                    continue;
                                }
                                s = ts;
                                dr[dc.ColumnName] = s;
                            }
                        }
                        len = Stuff.Max(len, s.Length);
                    }
                }
                dc.IsNullable = hasNulls;
                dc.MaxLength = Math.Max(1, len);
            }
        }

        public static string GenerateCreateTableSQL(this IDataTable dt, string schema = "dbo", string extraColumnSql=null)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("create table {0}.{1}\n(\n", schema, dt.TableName);
            for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = dt.Columns[colNum];
                string sqlType;
                if (dc.DataType == typeof(int))
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
                else if (dc.DataType == typeof(Byte))
                {
                    sqlType = "tinyint";
                }
                else if (dc.DataType == typeof(Int16))
                {
                    sqlType = "smallint";
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
                    dc.IsNullable ? "NULL" : "NOT NULL",
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
