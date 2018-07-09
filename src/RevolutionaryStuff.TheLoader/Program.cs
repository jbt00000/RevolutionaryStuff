﻿using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Collections;
using RevolutionaryStuff.Core.Diagnostics;
using RevolutionaryStuff.ETL;
using Simple.OData.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.TheLoader
{
    public class Program : CommandLineProgram
    {
        public enum Modes
        {
            Import,
            Export
        }

        private const string NameofModeImport = nameof(Modes.Import);
        private const string NameofModeExport = nameof(Modes.Export);

        #region Command Line Args

        [CommandLineSwitchModeSwitch(CommandLineSwitchAttribute.CommonArgNames.Mode)]
        public Modes Mode = Modes.Import;

        [CommandLineSwitch("RemoteServerType", Mandatory = false)]
        public RemoteServerTypes RemoteServerType = RemoteServerTypes.SqlServer;

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

        [CommandLineSwitch("Schema", Mandatory = true, Mode = NameofModeImport)]
        public string Schema;

        [CommandLineSwitch("Table", Mandatory = false, Mode = NameofModeImport)]
        public string Table;

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

        [CommandLineSwitch("CsvQuoteChar", Mandatory = false, Mode = NameofModeImport)]
        public char CsvQuoteChar = '"';

        [CommandLineSwitch("TrimAndNullifyStringData", Mandatory = false, Mode = NameofModeImport)]
        public bool TrimAndNullifyStringData = true;

        [CommandLineSwitch("RowNumberColumn", Mandatory = false, Description = "When specified, the row number from the load should be added here")]
        public string RowNumberColumnName;

        [CommandLineSwitch("MaxErrorRate", Mandatory = false, Mode = NameofModeImport)]
        public float MaxErrorRate = 0;

        #endregion

        public HashSet<string> SkipCols = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);

        private string ConnectionString;

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
            ConnectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
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
            Trace.WriteLine(e.Message);
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
                                        c.ColumnName = DataTableHelpers.OnDuplicateAppendSeqeuntialNumber(dt, c.ColumnName);
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
                                Trace.WriteLine(string.Format("Exported {0} rows from table {1}...",
                                                        dt.Rows.Count,
                                                        tableNum
                                                        ));
                            }
                        }
                        if (dt != null)
                        {
                            Trace.WriteLine(string.Format("Exported {0} rows from table {1}", dt.Rows.Count, tableNum));
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
                    Trace.WriteLine($"Downloading [{SourceUrl}] to [{FilePath}]");
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
                                col.ExtendedProperties["NumericPrecision"] = (short)et.Precision;
                            }
                            if (et.Scale.HasValue)
                            {
                                col.ExtendedProperties["NumericScale"] = (short)et.Scale;
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
                                col.ExtendedProperties["Unicode"] = srt.IsUnicode.Value;
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

        private async Task OnImportAsync()
        {
            Table = Stuff.CoalesceStrings(Table, MakeFriendly(Path.GetFileNameWithoutExtension(FilePath)));

            //Suck entire table into RAM
            var rowErrors = new List<Tuple<int, Exception>>();
            DataSet ds = null;
            DataTable dt = null;
            IList<string[]> st = null;
            if (FileFormat == FileFormats.Auto)
            {
                FileFormat = FileFormatHelpers.GetImpliedFormat(FilePath, SourceUrl);
            }
            if (FileFormat == FileFormats.ELF)
            {
                dt = ExtendedLogFileFormatHelpers.Load(File.OpenRead(FilePath), SkipCols);
                ThrowNowSupportedWhenOptionSpecified(nameof(RowNumberColumnName), nameof(SkipRawRows), nameof(ColumnNames));
                goto CleanDataTable;
            }
            else if (FileFormat == FileFormats.MySqlDump)
            {
                ds = MySqlHelpers.LoadDump(File.OpenRead(FilePath));
                ThrowNowSupportedWhenOptionSpecified(nameof(RowNumberColumnName), nameof(SkipRawRows), nameof(ColumnNames));
                goto CreateTable;
            }
            else if (FileFormat == FileFormats.FoxPro)
            {
                Stream memoStream = null;
                var memoPath = Path.ChangeExtension(FilePath, ".fpt");
                if (File.Exists(memoPath))
                {
                    memoStream = File.OpenRead(memoPath);
                }
                dt = DBaseHelpers.Load(File.OpenRead(FilePath), memoStream);
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
                dt = DataTableHelpers.LoadRowsFromDelineatedText(File.OpenRead(FilePath), new LoadRowsFromDelineatedTextSettings
                {
                    SkipRawRows = SkipRawRows,
                    Format = FileFormat == FileFormats.CSV ? LoadRowsFromDelineatedTextFormats.CommaSeparatedValues : LoadRowsFromDelineatedTextFormats.PipeSeparatedValues,
                    DuplicateColumnRenamer = DataTableHelpers.OnDuplicateAppendSeqeuntialNumber,
                    ColumnNames = ColumnNames,
                    ColumnNameTemplate = ColumnNameTemplate,
                    RowNumberColumnName = RowNumberColumnName,
                });
            }
            else if (FileFormat == FileFormats.CustomText)
            {
                dt = DataTableHelpers.LoadRowsFromDelineatedText(File.OpenRead(FilePath), new LoadRowsFromDelineatedTextSettings
                {
                    SkipRawRows = SkipRawRows,
                    CustomFieldDelim = CsvFieldDelim,
                    CustomQuoteChar = CsvQuoteChar,
                    DuplicateColumnRenamer = DataTableHelpers.OnDuplicateAppendSeqeuntialNumber,
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
                DataTableHelpers.LoadRowsFromFixedWidthText(dt, File.OpenRead(FilePath), loadSettings);
            }
            else if (FileFormat == FileFormats.Excel)
            {
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
                        SheetNumber = 0
                    } };
                }
                foreach (var rs in loadSettings.SheetSettings)
                {
                    rs.DuplicateColumnRenamer = DataTableHelpers.OnDuplicateAppendSeqeuntialNumber;
                    rs.RowNumberColumnName = RowNumberColumnName;
                }
                ds = new DataSet();
                ETL.SpreadsheetHelpers.LoadSheetsFromExcel(ds, File.OpenRead(FilePath), loadSettings);
            }
            else if (FileFormat == FileFormats.Json)
            {
                await DownloadFromSourceUrlAsync(".json");
                var json = File.ReadAllText(FilePath);
                var items = JsonConvert.DeserializeObject<JObject[]>(json);
                dt = DataTableHelpers.LoadRowsFromJObjects(null, items);
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
                if (ODataElementNames == null || ODataElementNames.Length==0)
                {
                    foreach (var el in m.EntityContainer.Elements)
                    {
                        entityNames.Add(el.Name);
                    }
                }
                else
                {
                    entityNames.AddRange(ODataElementNames);
                }
                ds = new DataSet();
                Parallel.ForEach(entityNames, new ParallelOptions{ MaxDegreeOfParallelism = 32 }, entityName => {
                    Trace.WriteLine($"Loading entity [{entityName}]");
                    var edt = new DataTable();
                    var z = (Microsoft.OData.Edm.IEdmStructuredType)m.FindDeclaredType($"{ODataElementSchema}.{entityName}");
                    var myClient = new ODataClient(new ODataClientSettings
                    {
                        BaseUri = SourceUrl,
                        MetadataDocument = mdoc
                    });

                    /*
                    var count = myClient
                            .For(entityName)
                        .Count()
                        .FindScalarAsync<int>().ExecuteSynchronously();

                    */

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
                            Trace.WriteLine(odataFailedException);
                            return;
                        }
                        edt = LoadRows(edt, entityName, z, items);
                        if (items.Count == 0) break;
                        startAt += items.Count;
                        Trace.WriteLine($"\tTable={entityName} Download Batch={items.Count}.  Running Count={startAt}/???");
                    }
                    if (entityNames.Count() == 1)
                    {
                        edt.TableName = StringHelpers.TrimOrNull(Table) ?? edt.TableName;
                    }
                    else
                    {
                        edt.TableName = entityName;
                    }
                    lock (ds)
                    {
                        Trace.WriteLine($"Added [{entityName}] {ds.Tables.Count}/{entityNames.Count}");
                        ds.Tables.Add(edt);
                    }
                });
            }
            else
            {
                throw new UnexpectedSwitchValueException(FileFormat);
            }

            switch (ColumnRenamingMode)
            {
                case ColumnRenamingModes.Preserve:
                    break;
                case ColumnRenamingModes.UpperCamelNoSpecialCharacters:
                    for (int colNum = 0; colNum < st[SkipRawRows].Length; ++colNum)
                    {
                        var colName = st[SkipRawRows][colNum];
                        if (colName == null) continue;
                        colName = MakeFriendly(colName);
                        st[SkipRawRows][colNum] = colName;
                    }
                    break;
                default:
                    throw new UnexpectedSwitchValueException(ColumnRenamingMode);
            }

            CleanDataTable:
            if (rowErrors.Count > 0)
            {
                float actualErrorRate = rowErrors.Count / (float)(dt.Rows.Count + rowErrors.Count);
                Trace.WriteLine(string.Format("There were {0} errors\nMax Error Rate: {1}\nActual Error Rate: {2}\nOn Rows: {3}", rowErrors.Count, MaxErrorRate, actualErrorRate, rowErrors.ConvertAll(t => t.Item1).Format(", ")));
                if (actualErrorRate > MaxErrorRate)
                {
                    throw new Exception(string.Format("Max Error rate exceeded!"));
                }
            }

            //Create the table SQL
            CreateTable:
            if (ds == null)
            {
                Requires.NonNull(dt, nameof(dt));
                dt.TableName = Stuff.CoalesceStrings(dt.TableName, Table);
                ds = new DataSet();
                ds.Tables.Add(dt);
            }
            Load(ds);
        }


        private void Load(DataSet ds)
        {
            int tableNum = 0;
            var po = new ParallelOptions { };
            if (!Parallelism)
            {
                po.MaxDegreeOfParallelism = 1;
            }
            Parallel.ForEach(ds.Tables.OfType<DataTable>(), po, dt => 
            {
                Interlocked.Increment(ref tableNum);
                foreach (var colName in SkipCols)
                {
                    dt.Columns.Remove(colName);
                }
                dt.IdealizeStringColumns(TrimAndNullifyStringData);

                using (new TraceRegion($"Operating on {Schema}.{dt.TableName}; Table {tableNum}/{ds.Tables.Count}; RemoteServerType={RemoteServerType}"))
                {
                    if (dt.Rows.Count == 0 && SkipZeroRowTables) return;
                    switch (RemoteServerType)
                    {
                        case RemoteServerTypes.SqlServer:
                            LoadIntoSqlServer(dt);
                            break;
/*
                        case RemoteServerTypes.DocumentDB:
                            LoadIntoDocumentDB(dt);
                            break;
*/
                        default:
                            throw new UnexpectedSwitchValueException(RemoteServerType);
                    }
                }
            });
        }

        private void LoadIntoSqlServer(DataTable dt)
        {
            dt.MakeDateColumnsFitSqlServerBounds();
            dt.MakeColumnNamesSqlServerFriendly();
            var sql = dt.GenerateCreateTableSQL(Schema, autoNumberColumnName: AutoNumberColumnName);
            Trace.WriteLine(sql);

            //Create table and insert 1 batch at a time
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.InfoMessage += Conn_InfoMessage;
                conn.Open();
                conn.ExecuteNonQuerySql(sql);
                var copy = new SqlBulkCopy(conn);
                copy.BulkCopyTimeout = 60 * 60 * 4;
                copy.DestinationTableName = $"[{Schema}].[{dt.TableName}]";
                copy.NotifyAfter = NotifyIncrement;
                copy.SqlRowsCopied += (sender, e) => Trace.WriteLine($"{copy.DestinationTableName} uploaded {e.RowsCopied}/{dt.Rows.Count} rows...");
                foreach (DataColumn dc in dt.Columns)
                {
                    copy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                }
                copy.WriteToServer(dt);
                copy.Close();
                Trace.WriteLine($"{copy.DestinationTableName} uploaded {dt.Rows.Count}/{dt.Rows.Count} rows.  Upload is complete.");
            }
        }

        static void Main(string[] args)
            => Main<Program>(args);
    }
}
