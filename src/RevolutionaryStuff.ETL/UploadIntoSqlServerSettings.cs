namespace RevolutionaryStuff.Data.ETL;

public class UploadIntoSqlServerSettings : UploadIntoDatastoreSettings
{
    public string Schema { get; set; } = "dbo";
    public bool GenerateTable { get; set; }
}
