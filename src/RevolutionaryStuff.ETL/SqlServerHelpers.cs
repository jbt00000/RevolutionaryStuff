using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using RevolutionaryStuff.Core.Data.SqlServer.Database;

namespace RevolutionaryStuff.Data.ETL;

public static class SqlServerHelpers
{
    extension(DataTable source)
    {
        public void UploadIntoSqlServer(Func<SqlConnection> createConnection, UploadIntoSqlServerSettings settings = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            settings ??= new UploadIntoSqlServerSettings();

            //Create table and insert 1 batch at a time
            using (var conn = createConnection())
            {
                conn.Open();

                if (settings.GenerateTable)
                {
                    var sql = source.GenerateCreateTableSQL(settings.Schema);
                    Trace.WriteLine(sql);
                    conn.ExecuteNonQuery(sql);
                }

                var copy = new SqlBulkCopy(conn)
                {
                    BulkCopyTimeout = 60 * 60 * 4,
                    DestinationTableName = $"[{settings.Schema}].[{source.TableName}]",

                    NotifyAfter = settings.RowsTransferredNotifyIncrement
                };
                copy.SqlRowsCopied += (sender, e) => Trace.WriteLine(string.Format("Uploaded {0}/{1} rows",
                    e.RowsCopied,
                    source.Rows.Count
                    ));
                foreach (DataColumn dc in source.Columns)
                {
                    copy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                }
                if (settings.RowsTransferredEventHandler != null)
                {
                    copy.SqlRowsCopied += (a, b) =>
                    {
                        var e = new RowsTransferredEventArgs(b.RowsCopied);
                        settings.RowsTransferredEventHandler(a, e);
                        b.Abort = e.Abort;
                    };
                }
                var dtr = source.CreateDataReader();
                copy.WriteToServer(dtr);
                copy.Close();
            }
            Trace.WriteLine($"Uploaded {source.Rows.Count} rows");
        }
    }
}
