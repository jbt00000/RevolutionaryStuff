using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Diagnostics;
using RevolutionaryStuff.ETL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        [CommandLineSwitch("Filename", Mandatory = true, Translator = CommandLineSwitchAttributeTranslators.FilePath)]
        public string FilePath;

        [CommandLineSwitch("FixedWidthColumnsFilename", Mandatory = false, Translator = CommandLineSwitchAttributeTranslators.FilePath)]
        public string FixedWidthColumnsFilePath;

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
            if (SkipColsArr != null)
            {
                foreach (var s in SkipColsArr) SkipCols.Add(s);
            }
            ConnectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
        }

        protected override Task OnGoAsync()
        {
            switch (Mode)
            {
                case Modes.Import:
                    OnImport();
                    break;
                case Modes.Export:
                    OnExport();
                    break;
                default:
                    throw new UnexpectedSwitchValueException(Mode);
            }
            return Task.CompletedTask;
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

        private void OnImport()
        {
            Table = Stuff.CoalesceStrings(Table, MakeFriendly(Path.GetFileNameWithoutExtension(FilePath)));

            //Suck entire table into RAM
            var rowErrors = new List<Tuple<int, Exception>>();
            DataSet ds = null;
            DataTable dt = null;
            IList<string[]> st = null;
            if (FileFormat == FileFormats.Auto)
            {
                var ext = Path.GetExtension(FilePath).ToLower();
                switch (ext)
                {
                    case ".dbf":
                        FileFormat = FileFormats.FoxPro;
                        break;
                    case ".csv":
                        FileFormat = FileFormats.CSV;
                        break;
                    case ".pipe":
                        FileFormat = FileFormats.Pipe;
                        break;
                    case ".log":
                        FileFormat = FileFormats.ELF;
                        break;
                    case ".xls":
                    case ".xlsx":
                        FileFormat = FileFormats.Excel;
                        break;
                    case ".mdmp":
                        FileFormat = FileFormats.MySqlDump;
                        break;
                    default:
                        throw new UnexpectedSwitchValueException(ext);
                }
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
            var sql = dt.GenerateCreateTableSQL(Schema);
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
                copy.WriteToServer(dt);
                copy.Close();
                Trace.WriteLine($"{copy.DestinationTableName} uploaded {dt.Rows.Count}/{dt.Rows.Count} rows.  Upload is complete.");
            }
        }

        static void Main(string[] args)
            => Main<Program>(args);
    }
}
