namespace RevolutionaryStuff.Data.ETL;

public class LoadRowsFromFixedWidthTextSettings : LoadRowsSettings
{
    public IList<IFixedWidthColumnInfo> ColumnInfos { get; set; }
}
