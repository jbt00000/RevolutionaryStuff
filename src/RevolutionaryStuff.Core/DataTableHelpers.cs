using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace RevolutionaryStuff.Core
{
    public static class DataTableHelpers
    {
        public static class CommonDataColumnExtendedPropertyNames
        {
            public const string Unicode = "Unicode";
            public const string NumericPrecision = "NumericPrecision";
            public const string NumericScale = "NumericScale";
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

        private static T ExtendedProperty<T>(this DataColumn dc, string propertyName, T? val=null, T missingValue=default(T)) where T : struct
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
                        return (T) p;
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

        public static bool Unicode(this DataColumn dc, bool? isUnicode = null)
            => dc.BooleanExtendedProperty(CommonDataColumnExtendedPropertyNames.Unicode, isUnicode, true);

        public static int NumericScale(this DataColumn dc, int? val = null)
            => dc.IntegerExtendedProperty(CommonDataColumnExtendedPropertyNames.NumericScale, val);

        public static int NumericPrecision(this DataColumn dc, int? val = null)
            => dc.IntegerExtendedProperty(CommonDataColumnExtendedPropertyNames.NumericPrecision, val);

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
    }
}
