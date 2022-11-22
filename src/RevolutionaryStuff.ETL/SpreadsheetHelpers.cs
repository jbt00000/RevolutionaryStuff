using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace RevolutionaryStuff.ETL;

public static partial class SpreadsheetHelpers
{
    public static object ExcelTypeConverter(object val, Type t)
    {
        try
        {
            var sval = val as string;
            if (t == typeof(DateTime) && sval != null)
            {
                if (double.TryParse(sval, out var d))
                {
                    return DateTime.FromOADate(d);
                }
            }
            else if (t == typeof(Decimal) && sval != null)
            {
                //https://stackoverflow.com/questions/22291165/parsing-decimal-in-scientific-notation
                if (Decimal.TryParse(sval, System.Globalization.NumberStyles.Float, null, out var d))
                {
                    return d;
                }
            }
            return Convert.ChangeType(val, t);
        }
        catch (Exception ex)
        {
            if (t == typeof(DateTime))
            {
                return DateTime.FromOADate(Convert.ToDouble(val));
            }
            throw ex;
        }
    }

    public static void ToSpreadSheet(this DataSet ds, string path)
    {
        using var st = File.Create(path);
        Core.SpreadsheetHelpers.ToSpreadSheet(ds, st);
    }

    private static void AddDefaultStylesPart(WorkbookPart workbookpart)
    {
        //https://stackoverflow.com/questions/11116176/cell-styles-in-openxml-spreadsheet-spreadsheetml
        var stylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();
        var dst = stylesPart.GetStream();
        var st = typeof(SpreadsheetHelpers).Assembly.GetEmbeddedResourceAsStream("styles.xml");
        st.CopyTo(dst);
    }

    public static void ToSpreadSheet(this DataSet ds, Stream st)
    {
        using var sd = SpreadsheetDocument.Create(st, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
        // Add a WorkbookPart to the document.
        var workbookpart = sd.AddWorkbookPart();
        var workbook = workbookpart.Workbook = new Workbook();

        AddDefaultStylesPart(workbookpart);

        // Add Sheets to the Workbook.
        var sheets = workbook.AppendChild<Sheets>(new Sheets());

        var indexBySharedString = new Dictionary<string, int>();

        uint sheetNum = 0;
        foreach (DataTable dt in ds.Tables)
        {
            // Add a WorksheetPart to the WorkbookPart.
            var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            var worksheet = worksheetPart.Worksheet = new Worksheet(sheetData);

            // Append a new worksheet and associate it with the workbook.
            var sheet = new Sheet()
            {
                Id = sd.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = ++sheetNum,
                Name = dt.TableName
            };
            sheets.Append(sheet);

            var ssRow = new Row { RowIndex = 1 };
            foreach (DataColumn dc in dt.Columns)
            {
                var ssCell = new Cell() { CellReference = CreateCellReference(dc.Ordinal, 0), StyleIndex = 14 };
                //                        var ssCell = new Cell() { CellReference = CreateCellReference(dc.Ordinal, 0) };
                SetValue(ssCell, dc.ColumnName, indexBySharedString);
                ssRow.Append(ssCell);
            }
            sheetData.Append(ssRow);
            uint rowNum = 0;
            foreach (DataRow dr in dt.Rows)
            {
                ssRow = new Row { RowIndex = (++rowNum) + 1 };
                for (var colNum = 0; colNum < dt.Columns.Count; ++colNum)
                {
                    var ssCell = new Cell() { CellReference = CreateCellReference(colNum, (int)rowNum) };
                    SetValue(ssCell, dr[colNum], indexBySharedString);
                    ssRow.Append(ssCell);
                }
                sheetData.Append(ssRow);
            }

            //Freeze Pane
            //https://stackoverflow.com/questions/6428590/freeze-panes-in-openxml-sdk-2-0-for-excel-document
            var sheetviews = new SheetViews();
            worksheet.InsertAt(sheetviews, 0);
            var sv = new SheetView()
            {
                WorkbookViewId = 0
            };
            sv.Pane = new Pane() { VerticalSplit = 1D, TopLeftCell = "A2", ActivePane = PaneValues.BottomLeft, State = PaneStateValues.Frozen };
            sv.Append(new Selection() { Pane = PaneValues.BottomLeft });
            sheetviews.Append(sv);
            worksheet.AppendChild(new AutoFilter() { Reference = CreateCellReference(0, 0, dt.Columns.Count - 1, 0) });
        }

        workbookpart.Workbook.Save();

        SharedStringTableCreate(sd, indexBySharedString);

        sd.Save();

        sd.Close();
    }

    private static string CreateCellReference(int colStart, int rowStart, int colEnd, int rowEnd)
    {
        return CreateCellReference(colStart, rowStart) + ":" + CreateCellReference(colEnd, rowEnd);
    }

    private static string CreateCellReference(int col, int row)
    {
        var cr = "";
        for (; ; )
        {
            cr = ((char)('A' + col % 26)) + cr;
            if (col < 26) break;
            col = col / 26 - 1;
        }
        cr += (row + 1).ToString();
        return cr;
    }

    private static void SetValue(Cell cell, object val, IDictionary<string, int> indexBySharedString)
    {
        if (val == null || val == DBNull.Value) val = "";
        var t = val.GetType();
        if (t.IsNumber())
        {
            cell.CellValue = new CellValue(val.ToString());
            cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Number);
        }
        else if (t == typeof(bool))
        {
            cell.CellValue = new CellValue((bool)val ? "1" : "0");
            cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Boolean);
        }
        else if (t == typeof(DateTime))
        {
            //                cell.CellValue = new CellValue(((DateTime)val).Date.ToOADate().ToString());
            //                cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Date);
            cell.CellValue = new CellValue(((DateTime)val).Date.ToShortDateString());
            cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.String);
        }
        else
        {
            var sVal = (string)val;
            if (!indexBySharedString.TryGetValue(sVal, out var pos))
            {
                pos = indexBySharedString.Count;
                indexBySharedString[sVal] = pos;
            }
            cell.CellValue = new CellValue(pos.ToString());
            cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.SharedString);
        }
    }

    private static void SharedStringTableCreate(SpreadsheetDocument sd, IDictionary<string, int> indexBySharedString)
    {
        SharedStringTablePart shareStringPart;
        if (sd.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
        {
            shareStringPart = sd.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
        }
        else
        {
            shareStringPart = sd.WorkbookPart.AddNewPart<SharedStringTablePart>();
        }

        // If the part does not contain a SharedStringTable, create one.
        if (shareStringPart.SharedStringTable == null)
        {
            shareStringPart.SharedStringTable = new SharedStringTable();
        }

        if (shareStringPart.SharedStringTable.Elements<SharedStringItem>().Count() > 0)
        {
            throw new InvalidOperationException("The workbook's shared string table already has values");
        }

        var items = indexBySharedString.OrderBy(kvp => kvp.Value).ToList();

        var iLast = -1;
        foreach (var kvp in items)
        {
            if (kvp.Value != ++iLast)
            {
                throw new InvalidOperationException("The shared string dictionary has gaps or is not zero based");
            }
        }

        foreach (var kvp in items)
        {
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(kvp.Key)));
        }
        shareStringPart.SharedStringTable.Save();
    }

    public static void LoadSheetsFromExcel(this DataSet ds, Stream st, LoadTablesFromSpreadsheetSettings settings = null)
    {
        Requires.NonNull(ds);
        Requires.ReadableStreamArg(st);

        settings ??= new LoadTablesFromSpreadsheetSettings();
        using var sd = SpreadsheetDocument.Open(st, false);
        var sheetSettings = settings.SheetSettings;
        if (sheetSettings == null || sheetSettings.Count == 0)
        {
            sheetSettings = new List<LoadRowsFromSpreadsheetSettings>();
            for (var n = 0; n < sd.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Count(); ++n)
            {
                sheetSettings.Add(new LoadRowsFromSpreadsheetSettings(settings.LoadAllSheetsDefaultSettings) { SheetNumber = n, UseSheetNameForTableName = true, TypeConverter = ExcelTypeConverter });
            }
        }
        foreach (var ss in sheetSettings)
        {
            var dt = settings.CreateDataTable == null ? new DataTable() : settings.CreateDataTable();
            dt.LoadRowsFromExcel(sd, ss);
            ds.Tables.Add(dt);
        }
    }

    public static void LoadRowsFromExcel(this DataTable dt, Stream st, LoadRowsFromSpreadsheetSettings settings)
    {
        Requires.NonNull(dt);
        Requires.ReadableStreamArg(st);

        using (var sd = SpreadsheetDocument.Open(st, false))
        {
            dt.LoadRowsFromExcel(sd, settings ?? new LoadRowsFromSpreadsheetSettings { SheetNumber = 0 });
        }
    }

    private static void LoadRowsFromExcel(this DataTable dt, SpreadsheetDocument sd, LoadRowsFromSpreadsheetSettings settings)
    {
        Requires.ZeroRows(dt);
        Requires.NonNull(sd);
        Requires.NonNull(settings);

        var rows = new List<IList<object>>();

        var sharedStringDictionary = ConvertSharedStringTableToDictionary(sd);
        var sheets = sd.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
        var sheetNumber = 0;
        foreach (var sheet in sheets)
        {
            if (sheetNumber == settings.SheetNumber || 0 == string.Compare(settings.SheetName, sheet.Name, true))
            {
                if (settings.UseSheetNameForTableName)
                {
                    dt.TableName = sheet.Name;
                }
                var relationshipId = sheet.Id.Value;
                var worksheetPart = (WorksheetPart)sd.WorkbookPart.GetPartById(relationshipId);
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var eRows = sheetData.Descendants<Row>();
                foreach (var erow in eRows)
                {
CreateRow:
                    var row = new List<object>();
                    rows.Add(row);
                    foreach (var cell in erow.Descendants<Cell>())
                    {
                        var cr = GetColRowFromCellReference(cell.CellReference);
                        if (rows.Count <= cr.Item2) goto CreateRow;
                        while (row.Count < cr.Item1)
                        {
                            row.Add(null);
                        }
                        Debug.Assert(row.Count == cr.Item1);
                        var val = GetCellValue(sd, cell, settings.TreatAllValuesAsText, sharedStringDictionary);
                        row.Add(val);
                    }
                }
                GC.Collect();
                IEnumerable<IList<object>> positionnedRows;
                if (settings.SkipRawRows.HasValue)
                {
                    positionnedRows = rows.Skip(settings.SkipRawRows.Value);
                }
                else if (settings.SkipWhileTester != null)
                {
                    positionnedRows = rows.SkipWhile(settings.SkipWhileTester);
                }
                else
                {
                    positionnedRows = rows;
                }
                dt.LoadRowsInternal(positionnedRows, settings);
                return;
            }
            ++sheetNumber;
        }
        throw new Exception($"Sheet [{settings.SheetNumber ?? (object)settings.SheetName}] was not found");
    }

    private static readonly Regex ColRowExpr = new(@"\s*([A-Z]+)(\d+)\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static Tuple<int, int> GetColRowFromCellReference(string cellReference)
    {
        var m = ColRowExpr.Match(cellReference);
        var colNum = 0;
        var colRef = m.Groups[1].Value.ToLower();
        for (var z = 0; z < colRef.Length; ++z)
        {
            colNum = colNum * 26 + (colRef[z] - 'a' + 1);
        }
        return new Tuple<int, int>(colNum - 1, int.Parse(m.Groups[2].Value) - 1);
    }

    private static IDictionary<int, string> ConvertSharedStringTableToDictionary(SpreadsheetDocument document)
    {
        var d = new Dictionary<int, string>();
        var stringTablePart = document.WorkbookPart.SharedStringTablePart;
        var pos = 0;
        foreach (var el in stringTablePart.SharedStringTable.ChildElements)
        {
            d[pos++] = el.InnerText;
        }
        return d;
    }

    private static object GetCellValue(SpreadsheetDocument document, Cell cell, bool treatAllValuesAsText, IDictionary<int, string> sharedStringDictionary)
    {
        if (cell == null || cell.CellValue == null) return null;
        var value = cell.CellValue.InnerText;
        if (cell.DataType == null) return value;
        var t = cell.DataType.Value;
        if (treatAllValuesAsText && t != CellValues.SharedString)
        {
            return value;
        }
        switch (t)
        {
            case CellValues.String:
                return value;
            case CellValues.SharedString:
                return sharedStringDictionary[Int32.Parse(value)];
            case CellValues.Boolean:
                return value == "1";
            case CellValues.Number:
                return value;
            case CellValues.Date:
                return value;
            default:
                return value;
        }
    }
}
