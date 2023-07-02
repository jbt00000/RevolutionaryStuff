using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using RevolutionaryStuff.Core.Data.SqlServer.Database;

namespace RevolutionaryStuff.Data.ETL;

public static class SqlServerHelpers
{
    public static void UploadIntoSqlServer(this DataTable dt, Func<SqlConnection> createConnection, UploadIntoSqlServerSettings settings = null)
    {
        ArgumentNullException.ThrowIfNull(dt);
        settings ??= new UploadIntoSqlServerSettings();

        //Create table and insert 1 batch at a time
        using (var conn = createConnection())
        {
            conn.Open();

            if (settings.GenerateTable)
            {
                var sql = dt.GenerateCreateTableSQL(settings.Schema);
                Trace.WriteLine(sql);
                conn.ExecuteNonQuery(sql);
            }

            var copy = new SqlBulkCopy(conn)
            {
                BulkCopyTimeout = 60 * 60 * 4,
                DestinationTableName = $"[{settings.Schema}].[{dt.TableName}]",

                NotifyAfter = settings.RowsTransferredNotifyIncrement
            };
            copy.SqlRowsCopied += (sender, e) => Trace.WriteLine(string.Format("Uploaded {0}/{1} rows",
                e.RowsCopied,
                dt.Rows.Count
                ));
            foreach (DataColumn dc in dt.Columns)
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
            var dtr = dt.CreateDataReader();
            copy.WriteToServer(dtr);
            copy.Close();
        }
        Trace.WriteLine($"Uploaded {dt.Rows.Count} rows");
    }
}
