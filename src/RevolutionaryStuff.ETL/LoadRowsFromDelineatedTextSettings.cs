namespace RevolutionaryStuff.ETL;

public enum LoadRowsFromDelineatedTextFormats
{
    CommaSeparatedValues,
    PipeSeparatedValues,
    Custom,
}

public class LoadRowsFromDelineatedTextSettings : LoadRowsSettings
{
    public LoadRowsFromDelineatedTextFormats Format { get; set; } = LoadRowsFromDelineatedTextFormats.CommaSeparatedValues;

    public char CustomFieldDelim { get; set; }

    public char? CustomQuoteChar { get; set; }

    public int SkipRawRows { get; set; }

    public string[] ColumnNames { get; set; }

    public string ColumnNameTemplate { get; set; }
}
