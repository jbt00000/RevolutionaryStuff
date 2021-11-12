namespace RevolutionaryStuff.ETL;

public class LoadTablesFromSpreadsheetSettings : LoadTablesSettings
{
    public List<LoadRowsFromSpreadsheetSettings> SheetSettings;
    public LoadRowsSettings LoadAllSheetsDefaultSettings;
}
