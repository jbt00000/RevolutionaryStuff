namespace RevolutionaryStuff.ETL;

public class LoadRowsFromObjectsSettings : LoadRowsSettings
{
    public bool GetPropertiesFromRelection { get; set; } = true;
    public bool GetFieldsFromRelection { get; set; } = true;
    public bool ColumnsFromEachObject { get; set; } = false;
}
