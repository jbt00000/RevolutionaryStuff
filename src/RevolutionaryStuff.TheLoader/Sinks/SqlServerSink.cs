using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Database;
using RevolutionaryStuff.ETL;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.TheLoader.Sinks
{
    public class SqlServerSink : BaseSink
    {
        private readonly Func<SqlConnection> CreateConnection;
        private readonly UploadIntoSqlServerSettings Settings;

        public SqlServerSink(Func<SqlConnection> createConnection, UploadIntoSqlServerSettings settings, Program program, IOptions<Program.LoaderConfig.TableConfig> tableConfigOptions)
            : base(program, tableConfigOptions)
        {
            Requires.NonNull(createConnection, nameof(createConnection));

            CreateConnection = createConnection;
            Settings = settings ?? new UploadIntoSqlServerSettings();
        }

        protected AlreadyExistsActions TableAlreadyExistsAction;
        protected string AutoNumberColumnName;

        protected override void OnInitProgramSettings(Program program)
        {
            base.OnInitProgramSettings(program);

            TableAlreadyExistsAction = program.TableAlreadyExistsAction;
            AutoNumberColumnName = program.AutoNumberColumnName;
        }

        protected async override Task OnUploadAsync(DataTable dt)
        {
            var schemaTable = $"[{Settings.Schema}].[{dt.TableName}]";

            dt.MakeDateColumnsFitSqlServerBounds();
            dt.MakeColumnNamesSqlServerFriendly();

            //Create table and insert 1 batch at a time
            using (var conn = CreateConnection())
            {
                conn.InfoMessage += (sender, e) => Trace.TraceInformation(e.Message);

                if (conn.TableExists(dt.TableName, Settings.Schema))
                {
                    switch (TableAlreadyExistsAction)
                    {
                        case AlreadyExistsActions.Append:
                            Trace.TraceWarning($"{Settings.Schema}.{dt.TableName} already exists.  Will append.");
                            break;
                        case AlreadyExistsActions.Skip:
                            Trace.TraceWarning($"{Settings.Schema}.{dt.TableName} already exists.  Will skip.");
                            return;
                        default:
                            throw new UnexpectedSwitchValueException(TableAlreadyExistsAction);
                    }
                }
                else
                {
                    if (Settings.GenerateTable)
                    {
                        var sql = dt.GenerateCreateTableSQL(Settings.Schema, autoNumberColumnName: AutoNumberColumnName);
                        Trace.TraceInformation(sql);
                        conn.ExecuteNonQuery(sql);
                        foreach (var propertyName in dt.ExtendedProperties.Keys.OfType<string>())
                        {
                            var propertyValue = dt.ExtendedProperties[propertyName];
                            await conn.TablePropertySetAsync(dt.TableName, propertyName, propertyValue, Settings.Schema);
                        }
                    }
                    else
                    {
                        throw new Exception($"{schemaTable} does not exist but we have not been asked to generate it");
                    }
                }
                var copy = new SqlBulkCopy(conn);
                copy.BulkCopyTimeout = 60 * 60 * 4;
                copy.DestinationTableName = $"[{Settings.Schema}].[{dt.TableName}]";
                copy.NotifyAfter = Settings.RowsTransferredNotifyIncrement;
                copy.SqlRowsCopied += (sender, e) => Trace.TraceInformation($"{copy.DestinationTableName} uploaded {e.RowsCopied}/{dt.Rows.Count} rows...");
                foreach (DataColumn dc in dt.Columns)
                {
                    copy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                }
                await copy.WriteToServerAsync(dt);
                copy.Close();
                Trace.TraceInformation($"{copy.DestinationTableName} uploaded {dt.Rows.Count}/{dt.Rows.Count} rows.  Upload is complete.");
            }
        }
    }
}
