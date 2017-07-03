using System;

namespace RevolutionaryStuff.Core.Data
{
    public interface IDataColumn
    {
        IDataTable Table { get; }
        Type DataType { get; set; }
        bool IsNullable { get; set; }
        string ColumnName { get; set; }
        int MaxLength { get; set; }
        int Ordinal { get; }
    }
}
