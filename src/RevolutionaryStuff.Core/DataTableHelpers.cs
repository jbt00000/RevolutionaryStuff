﻿using System;
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
            public const string PreserveTypeInformation = "PreserveTypeInformation";
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

        public static bool PreserveTypeInformation(this DataColumn dc, bool? preserveTypeInformation = null)
            => dc.BooleanExtendedProperty(CommonDataColumnExtendedPropertyNames.PreserveTypeInformation, preserveTypeInformation, false);

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
}
