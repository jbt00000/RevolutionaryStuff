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
            var col = new DataColumn(columnAttribute?.Name ?? pi.Name, dbType)
            {
                AllowDBNull = csType.IsNullable()
            };
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
                row[col] = val ?? DBNull.Value;
            }
            dt.Rows.Add(row);
        }
        return dt;
    }

    extension(DataRow source)
    {
        public int GetInt(string columnName, int fallback = default, Func<object, int> converter = null)
        {
            try
            {
                var o = source[columnName];
                if (o != DBNull.Value && o != null)
                {
                    return TypeHelpers.IsNumber(o.GetType())
                        ? Convert.ToInt32(o)
                        : converter != null ? converter(o) : Parse.ParseInt32(o.ToString(), fallback);
                }
            }
            catch (Exception)
            { }
            return fallback;
        }
        public string GetString(string columnName, Func<object, string> converter = null)
        {
            var o = source[columnName];
            return o == DBNull.Value ? null : o is string sv ? sv : converter != null ? converter(o) : o.ToString();
        }
        public string ToCsv()
                => CSV.FormatLine(source.ItemArray, true);
    }



    extension(DataRowCollection sources)
    {
        public void RemoveWhere(Predicate<DataRow> removeQualifier)
        {
            var removes = new List<DataRow>();
            foreach (DataRow dr in sources)
            {
                if (removeQualifier(dr))
                {
                    removes.Add(dr);
                }
            }
            foreach (var dr in removes)
            {
                sources.Remove(dr);
            }
        }
        public void Sample(int size, Random r = null)
        {
            r ??= Stuff.Random;
            while (sources.Count > size)
            {
                var n = r.Next(0, sources.Count - 1);
                sources.RemoveAt(n);
            }
        }
        public IList<DataRow> ToList()
        {
            var rows = new List<DataRow>();
            foreach (var dr in rows)
            {
                rows.Add(dr);
            }
            return rows;
        }
    }



    extension(DataColumnCollection sources)
    {
        public void AddRange(IEnumerable<string> fieldNames)
        {
            var existing = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            foreach (DataColumn dc in sources)
            {
                existing.Add(dc.ColumnName);
            }
            foreach (var colName in fieldNames)
            {
                if (existing.Contains(colName)) continue;
                existing.Add(colName);
                sources.Add(colName);
            }
        }
    }



    extension(DataColumn source)
    {
        internal DataColumn Xerox()
        {
            var c = new DataColumn(source.ColumnName, source.DataType, source.Expression)
            {
                AllowDBNull = source.AllowDBNull,
                AutoIncrement = source.AutoIncrement,
                AutoIncrementSeed = source.AutoIncrementSeed,
                AutoIncrementStep = source.AutoIncrementStep,
                Caption = source.Caption,
                ColumnMapping = source.ColumnMapping,
                DateTimeMode = source.DateTimeMode,
                DefaultValue = source.DefaultValue,
                MaxLength = source.MaxLength,
                Namespace = source.Namespace,
                Prefix = source.Prefix,
                ReadOnly = source.ReadOnly,
                Unique = source.Unique
            };
            foreach (var propertyName in source.ExtendedProperties.Keys)
            {
                c.ExtendedProperties[propertyName] = source.ExtendedProperties[propertyName];
            }
            return c;
        }
        private T ExtendedProperty<T>(string propertyName, T? val = null, T missingValue = default) where T : struct
        {
            if (val != null)
            {
                source.ExtendedProperties[propertyName] = val.Value;
                return val.Value;
            }

            try
            {
                var p = source.ExtendedProperties[propertyName];
                if (p != null)
                {
                    return (T)p;
                }
            }
            catch (Exception)
            { }
            return missingValue;
        }
        private int IntegerExtendedProperty(string propertyName, int? val = null, int missingValue = 0)
                => source.ExtendedProperty(propertyName, val, missingValue);
        private bool BooleanExtendedProperty(string propertyName, bool? val = null, bool missingValue = false)
                => source.ExtendedProperty(propertyName, val, missingValue);
        public bool PreserveTypeInformation(bool? preserveTypeInformation = null)
                => source.BooleanExtendedProperty(CommonDataColumnExtendedPropertyNames.PreserveTypeInformation, preserveTypeInformation, false);
        public bool Unicode(bool? isUnicode = null)
                => source.BooleanExtendedProperty(CommonDataColumnExtendedPropertyNames.Unicode, isUnicode, true);
        public int NumericScale(int? val = null)
                => source.IntegerExtendedProperty(CommonDataColumnExtendedPropertyNames.NumericScale, val);
        public int NumericPrecision(int? val = null)
                => source.IntegerExtendedProperty(CommonDataColumnExtendedPropertyNames.NumericPrecision, val);
        public DataColumn Clone()
        {
            var c = new DataColumn(source.ColumnName, source.DataType, source.Expression, source.ColumnMapping);
            foreach (var propertyName in source.ExtendedProperties)
            {
                var propertyVal = source.ExtendedProperties[propertyName];
                c.ExtendedProperties[propertyName] = propertyVal;
            }
            return c;
        }
    }















    extension(DataTable source)
    {
        public async Task ToCsvAsync(StreamWriter sw, char fieldDelimChar = CSV.FieldDelimDefault)
        {
            var sb = new StringBuilder();
            CSV.FormatLine(sb, source.Columns.OfType<DataColumn>().ConvertAll(z => z.ColumnName), false, fieldDelimChar);
            await sw.WriteLineAsync(sb.ToString());
            foreach (DataRow r in source.Rows)
            {
                sb.Clear();
                CSV.FormatLine(sb, r.ItemArray, false, fieldDelimChar);
                await sw.WriteLineAsync(sb.ToString());
            }
        }
        public string ToCsv()
        {
            var sb = new StringBuilder();
            CSV.FormatLine(sb, source.Columns.OfType<DataColumn>().ConvertAll(z => z.ColumnName), true);
            foreach (DataRow r in source.Rows)
            {
                sb.Append(r.ToCsv());
            }
            return sb.ToString();
        }
        public DataTable UnPivot(ICollection<string> dimensionColumnNames, string pivotKeyColumnName, string pivotValueColumnName)
        {
            var updt = new DataTable(source.TableName);
            var frontCols = new List<DataColumn>();
            var zippyCols = new List<DataColumn>();
            var dcn = new HashSet<string>(dimensionColumnNames, Comparers.CaseInsensitiveStringComparer);
            foreach (DataColumn dc in source.Columns)
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
            foreach (DataRow sourceRow in source.Rows)
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








}
