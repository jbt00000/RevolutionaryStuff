using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Database;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace RevolutionaryStuff.ETL
{
    public static class SqlServerHelpers
    {
        public static void MakeDateColumnsFitSqlServerBounds(this DataTable dt, DateTime? minDate = null, DateTime? maxDate = null)
        {
            Requires.NonNull(dt, nameof(dt));

            var lower = minDate.GetValueOrDefault(SqlServerMinDateTime);
            var upper = maxDate.GetValueOrDefault(SqlServerMaxDateTime);

            for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = (DataColumn)dt.Columns[colNum];
                if (dc.DataType != typeof(DateTime)) continue;
                int changeCount = 0;
                for (int rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                {
                    var o = dt.Rows[rowNum][dc];
                    if (o == DBNull.Value) continue;
                    var val = (DateTime)o;
                    if (val < lower)
                    {
                        ++changeCount;
                        dt.Rows[rowNum][dc] = lower;
                    }
                    else if (val > upper)
                    {
                        ++changeCount;
                        dt.Rows[rowNum][dc] = upper;
                    }
                }
                Trace.WriteLine($"MakeDateColumnsFitSqlServerBounds table({dt.TableName}) column({dc.ColumnName}) {colNum}/{dt.Columns.Count} => {changeCount} changes");
            }
        }

        private static readonly DateTime SqlServerMinDateTime = new DateTime(1753, 1, 1);
        private static readonly DateTime SqlServerMaxDateTime = new DateTime(9999, 12, 31);

        public static void UploadIntoSqlServer(this DataTable dt, Func<SqlConnection> createConnection, UploadIntoSqlServerSettings settings = null)
        {
            Requires.NonNull(dt, nameof(dt));
            settings = settings ?? new UploadIntoSqlServerSettings();

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

                var copy = new SqlBulkCopy(conn);
                copy.BulkCopyTimeout = 60 * 60 * 4;
                copy.DestinationTableName = string.Format("[{0}].[{1}]", settings.Schema, dt.TableName);

                copy.NotifyAfter = settings.RowsTransferredNotifyIncrement;
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
            Trace.WriteLine(string.Format("Uploaded {0} rows", dt.Rows.Count));
        }
    }
}
