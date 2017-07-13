using System;
using System.Collections.Generic;
using System.Text;

namespace RevolutionaryStuff.ETL
{
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
            this.UseSheetNameForTableName = other.UseSheetNameForTableName;
            this.SheetNumber = other.SheetNumber;
            this.SheetName = other.SheetName;
            this.SkipRawRows = other.SkipRawRows;
            this.SkipWhileTester = other.SkipWhileTester;
            this.TreatAllValuesAsText = other.TreatAllValuesAsText;
        }
    }
}
