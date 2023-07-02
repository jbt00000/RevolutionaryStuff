using System.Data;

namespace RevolutionaryStuff.Data.ETL;

public class LoadTablesSettings
{
    public Func<DataTable> CreateDataTable;
}
