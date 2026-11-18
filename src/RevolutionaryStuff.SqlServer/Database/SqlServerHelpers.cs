using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using RevolutionaryStuff.Core.Database;

namespace RevolutionaryStuff.Core.Data.SqlServer.Database;

public static class SqlServerHelpers
{
    public const int MaxTableNameLength = 128;
    public const int MaxTableColumnNameLength = 128;
    public static readonly DateTime SqlServerMinDateTime = new(1753, 1, 1);
    public static readonly DateTime SqlServerMaxDateTime = new(9999, 12, 31);

    extension(DataTable source)
    {
        public void MakeDateColumnsFitSqlServerBounds(DateTime? minDate = null, DateTime? maxDate = null)
        {
            ArgumentNullException.ThrowIfNull(source);

            var lower = minDate.GetValueOrDefault(SqlServerMinDateTime);
            var upper = maxDate.GetValueOrDefault(SqlServerMaxDateTime);

            for (var colNum = 0; colNum < source.Columns.Count; ++colNum)
            {
                var dc = source.Columns[colNum];
                if (dc.DataType != typeof(DateTime)) continue;
                var changeCount = 0;
                for (var rowNum = 0; rowNum < source.Rows.Count; ++rowNum)
                {
                    var o = source.Rows[rowNum][dc];
                    if (o == DBNull.Value) continue;
                    var val = (DateTime)o;
                    if (val < lower)
                    {
                        ++changeCount;
                        source.Rows[rowNum][dc] = lower;
                    }
                    else if (val > upper)
                    {
                        ++changeCount;
                        source.Rows[rowNum][dc] = upper;
                    }
                }
                Trace.WriteLine($"MakeDateColumnsFitSqlServerBounds table({source.TableName}) column({dc.ColumnName}) {colNum}/{source.Columns.Count} => {changeCount} changes");
            }
        }
        public void MakeColumnNamesSqlServerFriendly()
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (DataColumn dc in source.Columns)
            {
                if (dc.ColumnName.Length > MaxTableColumnNameLength)
                {
                    dc.ColumnName = dc.ColumnName.TruncateWithMidlineEllipsis(MaxTableColumnNameLength);
                }
            }
        }
    }

    extension(SqlConnection source)
    {
        public async Task TablePropertySetAsync(string tableName, string propertyName, object propertyValue, string schemaName = null)
        {
            schemaName = SqlHelpers.SchemaOrDefault(schemaName);
            var ps = new SqlParameter[]
                {
                            new("@propertyName", propertyName){Direction=ParameterDirection.Input},
                            new("@tableSchema", schemaName){Direction=ParameterDirection.Input},
                            new("@tableName", tableName){Direction=ParameterDirection.Input},
                            new("@cnt", SqlDbType.Int){Direction=ParameterDirection.Output},
                };
            var cnt = (await source.ExecuteNonQueryAsync(null, "select @cnt=count(*) from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @tableSchema, N'TABLE', @tableName, null, null)", null, ps, CommandType.Text)).GetOutputParameterVal<int>();
            if (cnt > 0)
            {
                ps = new SqlParameter[]
                    {
                            new("@name", propertyName){Direction=ParameterDirection.Input},
                            new("@level0name", schemaName){Direction=ParameterDirection.Input},
                            new("@level1name", tableName){Direction=ParameterDirection.Input},
                            new("@level0type", "SCHEMA"){Direction=ParameterDirection.Input},
                            new("@level1type", "TABLE"){Direction=ParameterDirection.Input},
                    };
                await source.ExecuteNonQueryAsync(null, "sys.sp_dropextendedproperty", null, ps);
            }
            ps = new SqlParameter[]
                {
                            new("@name", propertyName){Direction=ParameterDirection.Input},
                            new("@level0name", schemaName){Direction=ParameterDirection.Input},
                            new("@level1name", tableName){Direction=ParameterDirection.Input},
                            new("@level0type", "SCHEMA"){Direction=ParameterDirection.Input},
                            new("@level1type", "TABLE"){Direction=ParameterDirection.Input},
                            new("@value", propertyValue){Direction=ParameterDirection.Input},
                };
            await source.ExecuteNonQueryAsync(null, "sys.sp_addextendedproperty", null, ps);
        }
    }


}
