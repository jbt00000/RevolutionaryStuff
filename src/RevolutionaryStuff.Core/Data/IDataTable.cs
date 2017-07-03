using System.Collections.Generic;

namespace RevolutionaryStuff.Core.Data
{
    public interface IDataTable
    {
        string TableName { get; set; }
        IList<IDataColumn> Columns { get; }
        IDataRowCollection Rows { get; }
        IDataRow NewRow();
    }
}
