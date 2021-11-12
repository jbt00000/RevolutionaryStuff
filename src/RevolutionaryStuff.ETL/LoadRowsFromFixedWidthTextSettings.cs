namespace RevolutionaryStuff.ETL;

public class LoadRowsFromFixedWidthTextSettings : LoadRowsSettings
{
    public IList<IFixedWidthColumnInfo> ColumnInfos { get; set; }
}
