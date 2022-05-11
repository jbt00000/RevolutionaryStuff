using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;

namespace RevolutionaryStuff.Core;

public static class DataTableHelpers
{
    public static class CommonDataColumnExtendedPropertyNames
    {
        public const string Unicode = "Unicode";
        public const string NumericPrecision = "NumericPrecision";
        public const string NumericScale = "NumericScale";
        public const string PreserveTypeInformation = "PreserveTypeInformation";
    }

    public static DataTable ToDataTable<TVP>(IEnumerable<TVP> items)
    {
        var t = typeof(TVP);
        var dt = new DataTable();
        var tableAttribute = t.GetCustomAttribute<TableAttribute>();
        if (tableAttribute != null)
        {
            dt.TableName = tableAttribute.Name;
            dt.Namespace = tableAttribute.Schema;
        }
        var props = t.GetPropertiesPublicInstanceRead();
        var d = new Dictionary<PropertyInfo, DataColumn>();
        foreach (var pi in props)
        {
            if (pi.GetCustomAttribute<NotMappedAttribute>() != null) continue;

            var columnAttribute = pi.GetCustomAttribute<ColumnAttribute>();

            var csType = pi.GetUnderlyingType();
            var dbType = csType;
            if (dbType.IsGenericType && dbType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                dbType = dbType.GetGenericArguments()[0];
            }
            var col = new DataColumn(columnAttribute?.Name ?? pi.Name, dbType);
            col.AllowDBNull = csType.IsNullable();
            if (columnAttribute?.TypeName?.StartsWith("nvarchar", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                col.Unicode(true);
            }
            var maxLengthAttribute = pi.GetCustomAttribute<MaxLengthAttribute>();
            if (maxLengthAttribute != null)
            {
                col.MaxLength = maxLengthAttribute.Length;
            }
            dt.Columns.Add(col);
            d[pi] = col;
        }
        foreach (var item in items)
        {
            var row = dt.NewRow();
            foreach (var kvp in d)
            {
                var col = kvp.Value;
                var val = kvp.Key.GetValue(item);
                if (val == null)
                {
                    row[col] = DBNull.Value;
                }
                else
                {
                    row[col] = val;
                }
            }
            dt.Rows.Add(row);
        }
        return dt;
    }

    public static void RemoveWhere(this DataRowCollection rows, Predicate<DataRow> removeQualifier)
    {
        var removes = new List<DataRow>();
        foreach (DataRow dr in rows)
        {
            if (removeQualifier(dr))
            {
                removes.Add(dr);
            }
        }
        foreach (var dr in removes)
        {
            rows.Remove(dr);
        }
    }

    public static void Sample(this DataRowCollection rows, int size, Random r = null)
    {
        r ??= Stuff.Random;
        while (rows.Count > size)
        {
            var n = r.Next(0, rows.Count - 1);
            rows.RemoveAt(n);
        }
    }

    public static void AddRange(this DataColumnCollection dcc, IEnumerable<string> fieldNames)
    {
        var existing = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
        foreach (DataColumn dc in dcc)
        {
            existing.Add(dc.ColumnName);
        }
        foreach (var colName in fieldNames)
        {
            if (existing.Contains(colName)) continue;
            existing.Add(colName);
            dcc.Add(colName);
        }
    }

    public static IList<DataRow> ToList(this DataRowCollection rowCollection)
    {
        var rows = new List<DataRow>();
        foreach (DataRow dr in rowCollection)
        {
            rows.Add(dr);
        }
        return rows;
    }

    internal static DataColumn Xerox(this DataColumn dc)
    {
        var c = new DataColumn(dc.ColumnName, dc.DataType, dc.Expression)
        {
            AllowDBNull = dc.AllowDBNull,
            AutoIncrement = dc.AutoIncrement,
            AutoIncrementSeed = dc.AutoIncrementSeed,
            AutoIncrementStep = dc.AutoIncrementStep,
            Caption = dc.Caption,
            ColumnMapping = dc.ColumnMapping,
            DateTimeMode = dc.DateTimeMode,
            DefaultValue = dc.DefaultValue,
            MaxLength = dc.MaxLength,
            Namespace = dc.Namespace,
            Prefix = dc.Prefix,
            ReadOnly = dc.ReadOnly,
            Unique = dc.Unique
        };
        foreach (var propertyName in dc.ExtendedProperties.Keys)
        {
            c.ExtendedProperties[propertyName] = dc.ExtendedProperties[propertyName];
        }
        return c;
    }

    private static T ExtendedProperty<T>(this DataColumn dc, string propertyName, T? val = null, T missingValue = default(T)) where T : struct
    {
        if (val != null)
        {
            dc.ExtendedProperties[propertyName] = val.Value;
            return val.Value;
        }
        else
        {
            try
            {
                var p = dc.ExtendedProperties[propertyName];
                if (p != null)
                {
                    return (T)p;
                }
            }
            catch (Exception)
            { }
            return missingValue;
        }
    }

    private static int IntegerExtendedProperty(this DataColumn dc, string propertyName, int? val = null, int missingValue = 0)
        => dc.ExtendedProperty(propertyName, val, missingValue);

    private static bool BooleanExtendedProperty(this DataColumn dc, string propertyName, bool? val = null, bool missingValue = false)
        => dc.ExtendedProperty(propertyName, val, missingValue);

    public static bool PreserveTypeInformation(this DataColumn dc, bool? preserveTypeInformation = null)
        => dc.BooleanExtendedProperty(CommonDataColumnExtendedPropertyNames.PreserveTypeInformation, preserveTypeInformation, false);

    public static bool Unicode(this DataColumn dc, bool? isUnicode = null)
        => dc.BooleanExtendedProperty(CommonDataColumnExtendedPropertyNames.Unicode, isUnicode, true);

    public static int NumericScale(this DataColumn dc, int? val = null)
        => dc.IntegerExtendedProperty(CommonDataColumnExtendedPropertyNames.NumericScale, val);

    public static int NumericPrecision(this DataColumn dc, int? val = null)
        => dc.IntegerExtendedProperty(CommonDataColumnExtendedPropertyNames.NumericPrecision, val);

    public static async Task ToCsvAsync(this DataTable dt, StreamWriter sw, char fieldDelimChar = CSV.FieldDelimDefault)
    {
        var sb = new StringBuilder();
        CSV.FormatLine(sb, dt.Columns.OfType<DataColumn>().ConvertAll(z => z.ColumnName), false, fieldDelimChar);
        await sw.WriteLineAsync(sb.ToString());
        foreach (DataRow r in dt.Rows)
        {
            sb.Clear();
            CSV.FormatLine(sb, r.ItemArray, false, fieldDelimChar);
            await sw.WriteLineAsync(sb.ToString());
        }
    }

    public static string ToCsv(this DataTable dt)
    {
        var sb = new StringBuilder();
        CSV.FormatLine(sb, dt.Columns.OfType<DataColumn>().ConvertAll(z => z.ColumnName));
        foreach (DataRow r in dt.Rows)
        {
            sb.Append(r.ToCsv());
        }
        return sb.ToString();
    }

    public static string ToCsv(this DataRow dr)
        => CSV.FormatLine(dr.ItemArray, true);

    public static DataColumn Clone(this DataColumn dc)
    {
        var c = new DataColumn(dc.ColumnName, dc.DataType, dc.Expression, dc.ColumnMapping);
        foreach (var propertyName in dc.ExtendedProperties)
        {
            var propertyVal = dc.ExtendedProperties[propertyName];
            c.ExtendedProperties[propertyName] = propertyVal;
        }
        return c;
    }

    public static DataTable UnPivot(this DataTable dt, ICollection<string> dimensionColumnNames, string pivotKeyColumnName, string pivotValueColumnName)
    {
        var updt = new DataTable(dt.TableName);
        var frontCols = new List<DataColumn>();
        var zippyCols = new List<DataColumn>();
        var dcn = new HashSet<string>(dimensionColumnNames, Comparers.CaseInsensitiveStringComparer);
        foreach (DataColumn dc in dt.Columns)
        {
            if (dcn.Contains(dc.ColumnName))
            {
                updt.Columns.Add(dc.Clone());
                frontCols.Add(dc);
            }
            else
            {
                zippyCols.Add(dc);
            }
        }
        Requires.Positive(zippyCols.Count, $"{nameof(zippyCols)}.{nameof(zippyCols.Count)}");
        updt.Columns.Add(pivotKeyColumnName);
        updt.Columns.Add(pivotValueColumnName);
        foreach (DataRow sourceRow in dt.Rows)
        {
            foreach (var zippyCol in zippyCols)
            {
                var destRow = updt.NewRow();
                foreach (var dc in frontCols)
                {
                    destRow[dc.ColumnName] = sourceRow[dc];
                }
                destRow[pivotKeyColumnName] = zippyCol.ColumnName;
                destRow[pivotValueColumnName] = sourceRow[zippyCol];
                updt.Rows.Add(destRow);
            }
        }
        return updt;
    }
}
