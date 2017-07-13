using System;
using System.Data;

namespace RevolutionaryStuff.ETL
{
    public class LoadTablesSettings
    {
        public Func<DataTable> CreateDataTable;
    }
}
