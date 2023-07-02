namespace RevolutionaryStuff.Data.ETL;

public class LoadTablesFromSpreadsheetSettings : LoadTablesSettings
{
    public List<LoadRowsFromSpreadsheetSettings> SheetSettings;
    public LoadRowsSettings LoadAllSheetsDefaultSettings;
}
