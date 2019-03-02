using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Diagnostics;
using RevolutionaryStuff.ETL;
using RevolutionaryStuff.TheLoader.Uploaders;
using Simple.OData.Client;

namespace RevolutionaryStuff.TheLoader
{
    public class Program : CommandLineProgram
    {
        static void Main(string[] args)
            => Main<Program>(args);

        public enum Modes
        {
            Import,
            Export
        }

        public enum YesNoAuto
        {
            Yes,
            No,
            Auto,
        }

        private const string NameofModeImport = nameof(Modes.Import);
        private const string NameofModeExport = nameof(Modes.Export);

        #region Command Line Args

        [CommandLineSwitchModeSwitch(CommandLineSwitchAttribute.CommonArgNames.Mode)]
        public Modes Mode = Modes.Import;

        [CommandLineSwitch("SinkType", Mandatory = false)]
        public SinkTypes SinkType = SinkTypes.SqlServer;

        [CommandLineSwitch("SkipZeroRowTables", Mandatory = false, Mode = NameofModeImport)]
        public bool SkipZeroRowTables = true;

        [CommandLineSwitch("Filename", Mandatory = true, Translator = CommandLineSwitchAttributeTranslators.FilePathOrUrl)]
        public string FilePath;

        public Uri SourceUrl;

        [CommandLineSwitch("FixedWidthColumnsFilename", Mandatory = false, Translator = CommandLineSwitchAttributeTranslators.FilePath)]
        public string FixedWidthColumnsFilePath;

        [CommandLineSwitch("ODataElementSchema")]
        public string ODataElementSchema;

        [CommandLineSwitch("ODataElementNames", Translator = CommandLineSwitchAttributeTranslators.Csv)]
        public string[] ODataElementNames;

        [CommandLineSwitch("Sql", Mandatory = true, Mode = NameofModeExport)]
        public string Sql;

        [CommandLineSwitch("Schema", Mandatory = false, Mode = NameofModeImport)]
        public string SinkSchema="dbo";

        [CommandLineSwitch("Table", Mandatory = false, Mode = NameofModeImport)]
        public string SinkTable;

        [CommandLineSwitch("CSN", Mandatory = true)]
        public string ConnectionStringName;

        [CommandLineSwitch("ColumnNameFormat", Mandatory = false, Mode = NameofModeImport)]
        public ColumnNameFormats ColumnNameFormat = ColumnNameFormats.Auto;

        [CommandLineSwitch("FileFormat", Mandatory = false, Mode = NameofModeImport)]
        public FileFormats FileFormat = FileFormats.Auto;

        [CommandLineSwitch("ColumnRenamingMode", Mandatory = false, Mode = NameofModeImport)]
        public ColumnRenamingModes ColumnRenamingMode = ColumnRenamingModes.Preserve;

        [CommandLineSwitch("SheetNames", Mandatory = false, Mode = NameofModeImport, Translator = CommandLineSwitchAttributeTranslators.Csv)]
        public string[] SheetNames;

        [CommandLineSwitch("SkipCols", Mandatory = false, Translator = CommandLineSwitchAttributeTranslators.Csv)]
        public string[] SkipColsArr;

        [CommandLineSwitch("ColumnNames", Mandatory = false, Translator = CommandLineSwitchAttributeTranslators.Csv)]
        public string[] ColumnNames;

        [CommandLineSwitch("AutoFileNameColumnName", Mandatory = false)]
        public string AutoFileNameColumnName;

        [CommandLineSwitch("AutoNumberColumnName", Mandatory = false)]
        public string AutoNumberColumnName;

        [CommandLineSwitch("ColumnNameTemplate", Mandatory = false)]
        public string ColumnNameTemplate;

        [CommandLineSwitch("SkipRawRows", Mandatory = false, Description = "When a CSVish file, the number of raw rows to skip.  These should be prior to even the header row.")]
        public int SkipRawRows = 0;

        [CommandLineSwitch("Parallelism", Mandatory = false, Mode = NameofModeImport)]
        public bool Parallelism = true;

        [CommandLineSwitch("NotifyIncrement", Mandatory = false, Mode = NameofModeImport)]
        public int NotifyIncrement = 1000;

        [CommandLineSwitch("CsvFieldDelim", Mandatory = false, Mode = NameofModeImport)]
        public char CsvFieldDelim = ',';

        [CommandLineSwitch("SinkCsvFieldDelim", Mandatory = false, Mode = NameofModeImport)]
        public char SinkCsvFieldDelim = ',';

        [CommandLineSwitch("CsvQuoteChar", Mandatory = false, Mode = NameofModeImport)]
        public char CsvQuoteChar = '"';

        [CommandLineSwitch("TrimAndNullifyStringData", Mandatory = false, Mode = NameofModeImport)]
        public bool TrimAndNullifyStringData = true;

        [CommandLineSwitch("RightType", Mandatory = false, Mode = NameofModeImport)]
        public YesNoAuto RightType = YesNoAuto.Auto;

        [CommandLineSwitch("RowNumberColumn", Mandatory = false, Description = "When specified, the row number from the load should be added here")]
        public string RowNumberColumnName;

        [CommandLineSwitch("MaxErrorRate", Mandatory = false, Mode = NameofModeImport)]
        public float MaxErrorRate = 0;

        [CommandLineSwitch("TableAlreadyExistsAction", Mandatory = false, Mode = NameofModeImport)]
        public AlreadyExistsActions TableAlreadyExistsAction = AlreadyExistsActions.Skip;

        [CommandLineSwitch("UseSocrataMetadata", Mandatory = false, Mode = NameofModeImport)]
        public bool UseSocrataMetadata = false;

        [CommandLineSwitch("UnpivotLeftColumnNames", Mandatory = false, Mode = NameofModeImport, Translator = CommandLineSwitchAttributeTranslators.Csv)]
        public string[] UnpivotLeftColumnNames;

        [CommandLineSwitch("UnpivotKeyColumnName", Mandatory = false, Mode = NameofModeImport)]
        public string UnpivotKeyColumnName;

        [CommandLineSwitch("UnpivotValueColumnName", Mandatory = false, Mode = NameofModeImport)]
        public string UnpivotValueColumnName;

        #endregion

        public HashSet<string> SkipCols = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);

        public string ConnectionString;

        protected override void OnPostProcessCommandLineArgs()
        {
            base.OnPostProcessCommandLineArgs();
            Uri u;
            if (Uri.TryCreate(FilePath, UriKind.Absolute, out u))
            {
                SourceUrl = u;
            }
            if (SkipColsArr != null)
            {
                foreach (var s in SkipColsArr) SkipCols.Add(s);
            }
            switch (SinkType)
            {
                case SinkTypes.SqlServer:
                    ConnectionString = Configuration.GetConnectionString(ConnectionStringName);
                    break;
                default:
                    ConnectionString = Path.GetFullPath(ConnectionStringName);
                    break;
            }
        }

        protected override async Task OnGoAsync()
        {
            switch (Mode)
            {
                case Modes.Import:
                    await OnImportAsync();
                    break;
                case Modes.Export:
                    OnExport();
                    break;
                default:
                    throw new UnexpectedSwitchValueException(Mode);
            }
        }

        private string LastConnectionMessage;
        private void Conn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            LastConnectionMessage = e.Message;
            Trace.TraceInformation(e.Message);
        }

        private static readonly Regex TableNameExpr = new Regex(@"\Wtable:(\w+)\s*$", RegexOptions.IgnoreCase);

        private void OnExport()
        {
            var ds = new DataSet();
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.InfoMessage += Conn_InfoMessage;
                conn.Open();
                using (var cmd = new SqlCommand(Sql, conn)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 60 * 60
                })
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        int tableNum = 0;
                        ReadTable:
                        DataTable dt = null;
                        while (reader.Read())
                        {
                            if (dt == null)
                            {
                                var tableName = string.Format("Table{0}", tableNum);
                                if (LastConnectionMessage != null)
                                {
                                    tableName = LastConnectionMessage;
                                    LastConnectionMessage = null;
                                    var m = TableNameExpr.Match(tableName);
                                    if (m.Success)
                                    {
                                        tableName = m.Groups[1].Value;
                                    }
                                    else
                                    {
                                        tableName = RegexHelpers.Common.NonWordChars.Replace(LastConnectionMessage, " ");
                                        tableName = tableName.ToUpperCamelCase();
                                    }
                                }
                                dt = new DataTable(tableName);
                                ds.Tables.Add(dt);
                                var colsSeen = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
                                for (int z = 0; z < reader.FieldCount; ++z)
                                {
                                    var c = new DataColumn(reader.GetName(z), reader.GetFieldType(z));
                                    if (colsSeen.Contains(c.ColumnName))
                                    {
                                        c.ColumnName = DataLoadingHelpers.OnDuplicateAppendSeqeuntialNumber(dt, c.ColumnName);
                                    }
                                    colsSeen.Add(c.ColumnName);
                                    dt.Columns.Add(c);
                                }
                            }
                            var vals = new object[reader.FieldCount];
                            reader.GetValues(vals);
                            dt.Rows.Add(vals);
                            if (dt.Rows.Count % NotifyIncrement == 0)
                            {
                                Trace.TraceInformation(string.Format("Exported {0} rows from table {1}...",
                                                        dt.Rows.Count,
                                                        tableNum
                                                        ));
                            }
                        }
                        if (dt != null)
                        {
                            Trace.TraceInformation(string.Format("Exported {0} rows from table {1}", dt.Rows.Count, tableNum));
                        }
                        if (reader.NextResult())
                        {
                            ++tableNum;
                            goto ReadTable;
                        }
                    }
                }
                conn.Close();
                Stuff.FileTryDelete(this.FilePath);
                ds.ToSpreadSheet(this.FilePath);
            }
        }

        private static string MakeFriendly(string s)
        {
            var r = RegexHelpers.Create("[^0-9a-z]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s = r.Replace(s, " ");
            s = s.ToUpperCamelCase();
            return s;
        }

        private void ThrowNowSupportedWhenOptionSpecified(params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                var fi = GetType().GetField(fieldName);
                var val = fi.GetValue(this);
                if (val != null)
                {
                    if (!(val is string) && val is IEnumerable)
                    {
                        if (!((IEnumerable)val).HasData()) continue;
                    }
                    var cls = AttributeStuff.GetCustomAttribute<CommandLineSwitchAttribute>(fi.FieldType);
                    throw new NotSupportedException($"Option {cls?.AppSettingsName ?? fi.Name}=[{val}] is not supported with fileFormat={FileFormat}");
                }
            }
        }

        private async Task DownloadFromSourceUrlAsync(string localFileExtension)
        {
            if (SourceUrl != null)
            {
                FilePath = Stuff.GetTempFileName(localFileExtension);
                using (var client = new HttpClient())
                {
                    Trace.TraceInformation($"Downloading [{SourceUrl}] to [{FilePath}]");
                    using (var downloadStream = await client.GetStreamAsync(SourceUrl))
                    {
                        StreamHelpers.CopyTo(downloadStream, FilePath);
                    }
                }
            }
            else
            {
                Requires.FileExists(FilePath, nameof(FilePath));
            }
        }

        private static DataTable LoadRows(DataTable dt, string tableName, Microsoft.OData.Edm.IEdmStructuredType s, System.Collections.IEnumerable items)
        {
            dt = dt ?? new DataTable(tableName);
            if (dt.Columns.Count == 0)
            {
                var keyNames = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
                (s as IEdmEntityType)?.DeclaredKey?.ForEach(dk => keyNames.Add(dk.Name));
                foreach (var p in s.DeclaredProperties)
                {
                    var pt = (Microsoft.OData.Edm.IEdmTypeReference)p.Type;
                    var ptd = (Microsoft.OData.Edm.IEdmType)pt.Definition;
                    Stuff.Noop(pt, ptd);
                    var col = new DataColumn(p.Name)
                    {
                        AllowDBNull = p.Type.IsNullable
                    };
                    var pk = pt.PrimitiveKind();
                    switch (pk)
                    {
                        case EdmPrimitiveTypeKind.Boolean:
                            col.DataType = typeof(bool);
                            break;
                        case EdmPrimitiveTypeKind.Date:
                            col.DataType = typeof(DateTime);
                            break;
                        case EdmPrimitiveTypeKind.Decimal:
                            col.DataType = typeof(Decimal);
                            var et = pt.AsDecimal();
                            if (et.Precision.HasValue)
                            {
                                col.NumericPrecision(et.Precision.Value);
                            }
                            if (et.Scale.HasValue)
                            {
                                col.NumericScale(et.Scale.Value);
                            }
                            break;
                        case EdmPrimitiveTypeKind.Double:
                            col.DataType = typeof(double);
                            break;
                        case EdmPrimitiveTypeKind.Guid:
                            col.DataType = typeof(Guid);
                            break;
                        case EdmPrimitiveTypeKind.Int16:
                            col.DataType = typeof(Int16);
                            break;
                        case EdmPrimitiveTypeKind.Int32:
                            col.DataType = typeof(Int32);
                            break;
                        case EdmPrimitiveTypeKind.Int64:
                            col.DataType = typeof(Int64);
                            break;
                        case EdmPrimitiveTypeKind.SByte:
                            col.DataType = typeof(SByte);
                            break;
                        case EdmPrimitiveTypeKind.String:
                            col.DataType = typeof(string);
                            var srt = pt.AsString();
                            if (srt.MaxLength.HasValue)
                            {
                                col.MaxLength = srt.MaxLength.Value;
                            }
                            if (srt.IsUnicode.HasValue)
                            {
                                col.Unicode(srt.IsUnicode.Value);
                            }
                            break;
                        case EdmPrimitiveTypeKind.DateTimeOffset:
                            col.DataType = typeof(DateTimeOffset);
                            break;
                        case EdmPrimitiveTypeKind.None:
                            goto NextProperty;
                        default:
                            throw new NotSupportedException($"{pk} cannot be translated");
                    }
                    dt.Columns.Add(col);

                    if (keyNames.Contains(col.ColumnName))
                    {
                        var keyCols = new List<DataColumn>();
                        if (dt.PrimaryKey != null)
                        {
                            keyCols.AddRange(dt.PrimaryKey);
                        }
                        keyCols.Add(col);
                        //                        dt.PrimaryKey = keyCols.ToArray();
                    }
                    NextProperty:
                    Stuff.Noop();
                }
            }
            foreach (IDictionary<string, object> item in items)
            {
                var row = dt.NewRow();
                foreach (DataColumn col in dt.Columns)
                {
                    if (item.TryGetValue(col.ColumnName, out object val) && val != null)
                    {
                        row[col] = val;
                    }
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        private static string TrimPlus(string s)
        {
            s = s ?? "";
            s = RegexHelpers.Common.Whitespace.Replace(s, " ");
            s = s.Replace("&amp;", "&");
            return StringHelpers.TrimOrNull(s);
        }

        private async Task FigureItOutAsync()
        {
            var dt = new DataTable();
            dt.Columns.Add("id");
            dt.Columns.Add("title");
            dt.Columns.Add("href");
            dt.Columns.Add("category");
            dt.Columns.Add("description");
            dt.Columns.Add("title2");
            dt.Columns.Add("updated");
            dt.Columns.Add("odata");
            dt.Columns.Add("size");
            dt.Columns.Add("owner");

            int docNum = 0;

            var odc = new ODataClient(SourceUrl);
            var md = (Microsoft.OData.Edm.EdmModelBase)await odc.GetMetadataAsync();
            Parallel.ForEach(md.EntityContainer.Elements, Stuff.CreateParallelOptions(Parallelism), mde =>
            {
                var smd = SocrataMetadata.Fetch(SourceUrl, mde.Name);
                lock (dt)
                {
                    dt.Rows.Add(smd.Id, smd.Title, smd.PageUrl, smd.Category, smd.Description, smd.TableName, smd.Updated.ToYYYY_MM_DD(), smd.ODataUrl, smd.Size, smd.Owner);
                }
                Trace.TraceInformation($"docNum {docNum++}/{md.EntityContainer.Elements.Count()}");
            });
            var csv = dt.ToCsv();
            Trace.TraceInformation(csv);
        }

        private static async Task<Stream> OpenReadAsync(string fileName, bool? unzip=null)
        {
            using (new TraceRegion($"OpenReadAsync({fileName})"))
            {
                bool isZip = false;
                if (Uri.TryCreate(fileName, UriKind.Absolute, out var u))
                {
                    if (u.Scheme == WebHelpers.CommonSchemes.File)
                    {
                        fileName = u.LocalPath;
                    }
                    else
                    {
                        using (var client = new HttpClient())
                        {
                            Trace.WriteLine($"Downloading {u}");
                            var source = await client.GetStreamAsync(u);
                            fileName = Stuff.GetTempFileName(Stuff.CoalesceStrings(Path.GetExtension(u.LocalPath), ".tmp"));
                            Stuff.MarkFileForCleanup(fileName);
                            using (var dest = File.Create(fileName))
                            {
                                await source.CopyToAsync(dest);
                            }
                        }
                    }
                }
                isZip = isZip || MimeType.Application.Zip.DoesExtensionMatch(fileName);
                if (isZip && unzip.GetValueOrDefault(true))
                {
                    var dir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(fileName));
                    Trace.WriteLine($"Will unzip to {dir}");                    
                    System.IO.Compression.ZipFile.ExtractToDirectory(fileName, dir);
                    fileName = null;
                    foreach (var fn in Directory.GetFiles(dir))
                    {
                        fileName = fileName ?? fn;
                        Stuff.MarkFileForCleanup(fn);
                    }
                }
                Trace.WriteLine($"Opening {u}");
                return File.OpenRead(fileName);
            }
        }

        private static Stream OpenRead(string fileName, bool? unzip=null)
            => OpenReadAsync(fileName, unzip).ExecuteSynchronously();


        private void AddAutoFileNameColumnName(DataTable dt)
        {
            if (AutoFileNameColumnName != null)
            {
                var dc = new DataColumn(AutoFileNameColumnName, typeof(string)) { MaxLength = 255 };
                dc.PreserveTypeInformation(true);
                dt.Columns.Add(dc);
                string fileName = Path.GetFileName(this.FilePath);
                dt.SetColumnWithValue(this.AutoFileNameColumnName, (a, b) => fileName);
            }
        }

        private DataTable Unpivot(DataTable dt)
        {
            if (UnpivotLeftColumnNames!=null && UnpivotLeftColumnNames.Length > 0)
            {
                dt = dt.UnPivot(UnpivotLeftColumnNames, UnpivotKeyColumnName, UnpivotValueColumnName);
            }
            return dt;
        }

        private async Task OnImportAsync()
        {
            SinkTable = Stuff.CoalesceStrings(SinkTable, MakeFriendly(Path.GetFileNameWithoutExtension(FilePath)));

//            await FigureItOutAsync();

            //Suck entire table into RAM
            var rowErrors = new List<Tuple<int, Exception>>();
            DataSet ds = null;
            DataTable dt = null;
            if (FileFormat == FileFormats.Auto)
            {
                FileFormat = FileFormatHelpers.GetImpliedFormat(FilePath, SourceUrl);
            }
            if (FileFormat == FileFormats.Html)
            {
                RightType = RightType == YesNoAuto.Auto ? YesNoAuto.Yes : RightType;
                dt = HtmlTableFileFormatHelpers.Load(OpenRead(FilePath), new LoadRowsFromHtmlSettings
                {
                    DuplicateColumnRenamer = DataLoadingHelpers.OnDuplicateAppendSeqeuntialNumber,
                    RowNumberColumnName = RowNumberColumnName,
                });
                goto CleanDataTable;
            }
            else if (FileFormat == FileFormats.ELF)
            {
                dt = ExtendedLogFileFormatHelpers.Load(OpenRead(FilePath), SkipCols);
                ThrowNowSupportedWhenOptionSpecified(nameof(RowNumberColumnName), nameof(SkipRawRows), nameof(ColumnNames));
                goto CleanDataTable;
            }
            else if (FileFormat == FileFormats.MySqlDump)
            {
                ds = MySqlHelpers.LoadDump(OpenRead(FilePath));
                ThrowNowSupportedWhenOptionSpecified(nameof(RowNumberColumnName), nameof(SkipRawRows), nameof(ColumnNames));
                goto CreateTable;
            }
            else if (FileFormat == FileFormats.FoxPro)
            {
                Stream memoStream = null;
                var memoPath = Path.ChangeExtension(FilePath, ".fpt");
                if (File.Exists(memoPath))
                {
                    memoStream = OpenRead(memoPath);
                }
                dt = DBaseHelpers.Load(OpenRead(FilePath), memoStream);
                Stuff.Dispose(memoStream);
                switch (ColumnRenamingMode)
                {
                    case ColumnRenamingModes.Preserve:
                        break;
                    case ColumnRenamingModes.UpperCamelNoSpecialCharacters:
                        foreach (DataColumn dc in dt.Columns)
                        {
                            dc.ColumnName = MakeFriendly(dc.ColumnName);
                        }
                        break;
                    default:
                        throw new UnexpectedSwitchValueException(ColumnRenamingMode);
                }
                goto CreateTable;
            }
            else if (FileFormat == FileFormats.CSV || FileFormat == FileFormats.Pipe)
            {
                RightType = RightType == YesNoAuto.Auto ? YesNoAuto.Yes : RightType;
                dt = DataLoadingHelpers.LoadRowsFromDelineatedText(OpenRead(FilePath), new LoadRowsFromDelineatedTextSettings
                {
                    SkipRawRows = SkipRawRows,
                    Format = FileFormat == FileFormats.CSV ? LoadRowsFromDelineatedTextFormats.CommaSeparatedValues : LoadRowsFromDelineatedTextFormats.PipeSeparatedValues,
                    DuplicateColumnRenamer = DataLoadingHelpers.OnDuplicateAppendSeqeuntialNumber,
                    ColumnNames = ColumnNames,
                    ColumnNameTemplate = ColumnNameTemplate,
                    RowNumberColumnName = RowNumberColumnName,
                });
            }
            else if (FileFormat == FileFormats.CustomText)
            {
                RightType = RightType == YesNoAuto.Auto ? YesNoAuto.Yes : RightType;
                dt = DataLoadingHelpers.LoadRowsFromDelineatedText(OpenRead(FilePath), new LoadRowsFromDelineatedTextSettings
                {
                    SkipRawRows = SkipRawRows,
                    CustomFieldDelim = CsvFieldDelim,
                    CustomQuoteChar = CsvQuoteChar,
                    DuplicateColumnRenamer = DataLoadingHelpers.OnDuplicateAppendSeqeuntialNumber,
                    Format = LoadRowsFromDelineatedTextFormats.Custom,
                    ColumnNames = ColumnNames,
                    ColumnNameTemplate = ColumnNameTemplate,
                    RowNumberColumnName = RowNumberColumnName,
                });
            }
            else if (FileFormat == FileFormats.FixedWidthText)
            {
                var loadSettings = new LoadRowsFromFixedWidthTextSettings
                {
                    ColumnInfos = FixedWidthColumnInfo.CreateFromCsv(File.ReadAllText(FixedWidthColumnsFilePath))
                };
                DataLoadingHelpers.LoadRowsFromFixedWidthText(dt, OpenRead(FilePath), loadSettings);
            }
            else if (FileFormat == FileFormats.Excel)
            {
                RightType = RightType == YesNoAuto.Auto ? YesNoAuto.Yes : RightType;
                var loadSettings = new LoadTablesFromSpreadsheetSettings();
                if (SheetNames != null && SheetNames.Length > 0)
                {
                    loadSettings.SheetSettings = SheetNames.Distinct().ConvertAll(sheetName => new LoadRowsFromSpreadsheetSettings
                    {
                        UseSheetNameForTableName = true,
                        SheetName = sheetName,
                    }).ToList();
                }
                else
                {
                    loadSettings.SheetSettings = new List<LoadRowsFromSpreadsheetSettings> { new LoadRowsFromSpreadsheetSettings {
                        UseSheetNameForTableName = true,
                        SheetNumber = 0,
                    } };
                }
                foreach (var rs in loadSettings.SheetSettings)
                {
                    rs.DuplicateColumnRenamer = DataLoadingHelpers.OnDuplicateAppendSeqeuntialNumber;
                    rs.RowNumberColumnName = RowNumberColumnName;
                    rs.SkipRawRows = SkipRawRows;
                }
                ds = new DataSet();
                ETL.SpreadsheetHelpers.LoadSheetsFromExcel(ds, OpenRead(FilePath), loadSettings);
            }
            else if (FileFormat == FileFormats.Json)
            {
                await DownloadFromSourceUrlAsync(".json");
                var json = File.ReadAllText(FilePath);
                var items = JsonConvert.DeserializeObject<JObject[]>(json);
                dt = DataLoadingHelpers.LoadRowsFromJObjects(null, items);
            }
            else if (FileFormat == FileFormats.OData4)
            {
                //https://github.com/object/Simple.OData.Client/wiki
                //http://www.odata.org/blog/advanced-odata-tutorial-with-simple-odata-client/
                //http://www.nudoq.org/#!/Packages/Microsoft.OData.Edm/Microsoft.OData.Edm/IEdmTypeReference
                var client = new ODataClient(SourceUrl);
                var m = (Microsoft.OData.Edm.EdmModelBase)await client.GetMetadataAsync();
                var mdoc = await client.GetMetadataDocumentAsync();
                var entityNames = new List<string>();
                if (ODataElementNames == null || ODataElementNames.Length == 0)
                {
                    foreach (var el in m.EntityContainer.Elements)
                    {
                        entityNames.Add(el.Name);
                    }
                    entityNames.Sort();
                }
                else
                {
                    entityNames.AddRange(ODataElementNames);
                }
                ds = new DataSet();
                Parallel.ForEach(entityNames, Stuff.CreateParallelOptions(Parallelism, 32), entityName =>
                {
                    Trace.TraceInformation($"Loading entity [{entityName}]");
                    var edt = new DataTable(entityName);
                    var z = (Microsoft.OData.Edm.IEdmStructuredType)m.FindDeclaredType($"{ODataElementSchema}.{entityName}");
                    if (z == null)
                    {
                        Trace.TraceWarning($"cannot find entity [{entityName}] in schema");
                        return;
                    }
                    var myClient = new ODataClient(new ODataClientSettings
                    {
                        BaseUri = SourceUrl,
                        MetadataDocument = mdoc
                    });
                    int? totalRowCount = null;
                    if (UseSocrataMetadata)
                    {
                        try
                        {
                            var smd = SocrataMetadata.Fetch(SourceUrl, entityName);
                            edt.TableName = smd.TableName;
                            edt.ExtendedProperties["ForeignId"] = smd.Id;
                            edt.ExtendedProperties["PageUrl"] = Stuff.ObjectToString(smd.PageUrl);
                            edt.ExtendedProperties["ODataUrl"] = Stuff.ObjectToString(smd.ODataUrl);
                            edt.ExtendedProperties["Keywords"] = smd.Category;
                            edt.ExtendedProperties["Description"] = smd.Description;
                            edt.ExtendedProperties["SourceUpdatedAt"] = smd.Updated.ToYYYY_MM_DD();
                            edt.ExtendedProperties["Comment"] = smd.Description;
                            totalRowCount = smd.Size;
                        }
                        catch (Exception socEx)
                        {
                            Trace.TraceWarning($"Problem getting/parsing socrata info on [{SourceUrl}] for [{entityName}]\n{socEx.Message}");
                        }
                    }

                    if (totalRowCount.GetValueOrDefault() < 1)
                    {
                        try
                        {
                            totalRowCount = myClient
                                    .For(entityName)
                                .Count()
                                .FindScalarAsync<int>().ExecuteSynchronously();
                        }
                        catch (Exception rowCountEx)
                        {
                            Trace.WriteLine(rowCountEx);
                        }
                    }

                    int startAt = 0;
                    for (; ; )
                    {
                        List<IDictionary<string, object>> items;
                        try
                        {
                            items = DelegateHelpers.CallAndRetryOnFailure(() =>
                                myClient
                                .For(entityName)
                                .OrderBy(new[] { (z as IEdmEntityType)?.DeclaredKey?.FirstOrDefault()?.Name ?? z.DeclaredProperties.First().Name })
                                .Skip(startAt)
                                .FindEntriesAsync().ExecuteSynchronously().ToList()
                            );
                        }
                        catch (Exception odataFailedException)
                        {
                            Trace.TraceError(odataFailedException.ToString());
                            return;
                        }
                        edt = LoadRows(edt, entityName, z, items);
                        if (items.Count == 0) break;
                        startAt += items.Count;
                        Trace.TraceInformation($"\tTable={entityName} Download Batch={items.Count}.  Running Count={startAt}/{totalRowCount.GetValueOrDefault(-1)}");
                    }
                    if (entityNames.Count() == 1)
                    {
                        edt.TableName = StringHelpers.TrimOrNull(SinkTable) ?? edt.TableName;
                    }
                    lock (ds)
                    {
                        Trace.TraceInformation($"Added [{entityName}] {ds.Tables.Count}/{entityNames.Count}");
                        ds.Tables.Add(edt);
                    }
                });
            }
            else
            {
                throw new UnexpectedSwitchValueException(FileFormat);
            }

            CleanDataTable:
            if (rowErrors.Count > 0)
            {
                float actualErrorRate = rowErrors.Count / (float)(dt.Rows.Count + rowErrors.Count);
                Trace.TraceError(string.Format("There were {0} errors\nMax Error Rate: {1}\nActual Error Rate: {2}\nOn Rows: {3}", rowErrors.Count, MaxErrorRate, actualErrorRate, rowErrors.ConvertAll(t => t.Item1).Format(", ")));
                if (actualErrorRate > MaxErrorRate)
                {
                    throw new Exception(string.Format("Max Error rate exceeded!"));
                }
            }

            if (dt != null)
            {
                dt = Unpivot(dt);
            }

            //Create the table SQL
            CreateTable:
            if (ds == null)
            {
                Requires.NonNull(dt, nameof(dt));
                dt.TableName = Stuff.CoalesceStrings(dt.TableName, SinkTable);
                ds = new DataSet();
                ds.Tables.Add(dt);
            }

            foreach (DataTable zdt in ds.Tables)
            {
                AddAutoFileNameColumnName(zdt);
                switch (ColumnRenamingMode)
                {
                    case ColumnRenamingModes.Preserve:
                        break;
                    case ColumnRenamingModes.UpperCamelNoSpecialCharacters:
                        for (int colNum = 0; colNum < zdt.Columns.Count; ++colNum)
                        {
                            var dc = zdt.Columns[colNum];
                            dc.ColumnName = MakeFriendly(dc.ColumnName);
                        }
                        break;
                    default:
                        throw new UnexpectedSwitchValueException(ColumnRenamingMode);
                }
            }

            Load(ds);
        }


        private void Load(DataSet ds)
        {
            int tableNum = 0;
            Parallel.ForEach(ds.Tables.OfType<DataTable>(), Stuff.CreateParallelOptions(Parallelism), dt =>
            {
                Interlocked.Increment(ref tableNum);
                foreach (var colName in SkipCols)
                {
                    dt.Columns.Remove(colName);
                }
                if (RightType==YesNoAuto.Yes)
                {
                    dt.RightType();
                }
                else
                {
                    dt.IdealizeStringColumns(TrimAndNullifyStringData);
                }
                using (new TraceRegion($"Operating on {SinkSchema}.{dt.TableName}; Table {tableNum}/{ds.Tables.Count}; RemoteServerType={SinkType}"))
                {
                    if (dt.Rows.Count == 0 && SkipZeroRowTables) return;
                    IUploader uploader;
                    switch (SinkType)
                    {
                        case SinkTypes.SqlServer:
                            uploader = new SqlServerUploader(
                                this,
                                () => new SqlConnection(ConnectionString),
                                new UploadIntoSqlServerSettings { Schema = SinkSchema, GenerateTable = true, RowsTransferredNotifyIncrement = NotifyIncrement }
                                );
                            break;
                        case SinkTypes.FlatFile:
                            uploader = new FlatFileUploader(this);
                            break;
                        default:
                            throw new UnexpectedSwitchValueException(SinkType);
                    }
                    uploader.UploadAsync(dt).ExecuteSynchronously();
                }
            });
        }
    }
}
