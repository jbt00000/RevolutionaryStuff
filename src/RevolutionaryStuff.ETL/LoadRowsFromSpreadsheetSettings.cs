namespace RevolutionaryStuff.ETL;

public class LoadRowsFromSpreadsheetSettings : LoadRowsSettings
{
    public bool UseSheetNameForTableName { get; set; }
    public int? SheetNumber { get; set; }
    public string SheetName { get; set; }
    public int? SkipRawRows { get; set; }
    public Func<IList<object>, bool> SkipWhileTester { get; set; }
    public bool TreatAllValuesAsText { get; set; }

    public LoadRowsFromSpreadsheetSettings() { }

    public LoadRowsFromSpreadsheetSettings(LoadRowsSettings other)
        : base(other)
    { }

    public LoadRowsFromSpreadsheetSettings(LoadRowsFromSpreadsheetSettings other)
        : base(other)
    {
        if (other == null) return;
        UseSheetNameForTableName = other.UseSheetNameForTableName;
        SheetNumber = other.SheetNumber;
        SheetName = other.SheetName;
        SkipRawRows = other.SkipRawRows;
        SkipWhileTester = other.SkipWhileTester;
        TreatAllValuesAsText = other.TreatAllValuesAsText;
    }
}
