﻿using System.Data;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.ETL;

public class LoadRowsSettings : IValidate
{
    public Action<Exception, int> RowAddErrorHandler { get; set; }
    public string RowNumberColumnName { get; set; }
    public Func<DataTable, string, string> DuplicateColumnRenamer { get; set; }
    public Func<string, string> ColumnMapper { get; set; }
    public Func<object, Type, object> TypeConverter { get; set; }
    public Func<DataTable, object[], bool> ShouldAddRow { get; set; }

    public LoadRowsSettings()
    {
        TypeConverter = Convert.ChangeType;
        ShouldAddRow = DontAddEmptyRows;
    }

    public LoadRowsSettings(LoadRowsSettings other)
        : this()
    {
        if (other == null) return;
        RowAddErrorHandler = other.RowAddErrorHandler;
        RowNumberColumnName = other.RowNumberColumnName;
        DuplicateColumnRenamer = other.DuplicateColumnRenamer;
        ColumnMapper = other.ColumnMapper;
        TypeConverter = other.TypeConverter;
    }

    public static bool DontAddEmptyRows(DataTable dt, object[] row)
    {
        foreach (var v in row)
        {
            if (v != DBNull.Value && v != null && v.ToString() != "") return true;
        }
        return false;
    }

    public void Validate()
    { }
}
