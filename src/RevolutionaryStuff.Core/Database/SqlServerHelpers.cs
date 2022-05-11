using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace RevolutionaryStuff.Core.Database;

public static class SqlServerHelpers
{
    public const int MaxTableNameLength = 128;
    public const int MaxTableColumnNameLength = 128;
    public static readonly DateTime SqlServerMinDateTime = new(1753, 1, 1);
    public static readonly DateTime SqlServerMaxDateTime = new(9999, 12, 31);

    public static void MakeDateColumnsFitSqlServerBounds(this DataTable dt, DateTime? minDate = null, DateTime? maxDate = null)
    {
        Requires.NonNull(dt, nameof(dt));

        var lower = minDate.GetValueOrDefault(SqlServerMinDateTime);
        var upper = maxDate.GetValueOrDefault(SqlServerMaxDateTime);

        for (var colNum = 0; colNum < dt.Columns.Count; ++colNum)
        {
            var dc = dt.Columns[colNum];
            if (dc.DataType != typeof(DateTime)) continue;
            var changeCount = 0;
            for (var rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
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

    public static async Task TablePropertySetAsync(this SqlConnection conn, string tableName, string propertyName, object propertyValue, string schemaName = null)
    {
        schemaName = SqlHelpers.SchemaOrDefault(schemaName);
        var ps = new SqlParameter[]
            {
                    new("@propertyName", propertyName){Direction=ParameterDirection.Input},
                    new("@tableSchema", schemaName){Direction=ParameterDirection.Input},
                    new("@tableName", tableName){Direction=ParameterDirection.Input},
                    new("@cnt", SqlDbType.Int){Direction=ParameterDirection.Output},
            };
        var cnt = (await conn.ExecuteNonQueryAsync(null, "select @cnt=count(*) from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @tableSchema, N'TABLE', @tableName, null, null)", null, ps, CommandType.Text)).GetOutputParameterVal<int>();
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
            await conn.ExecuteNonQueryAsync(null, "sys.sp_dropextendedproperty", null, ps);
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
        await conn.ExecuteNonQueryAsync(null, "sys.sp_addextendedproperty", null, ps);
    }

    public static void MakeColumnNamesSqlServerFriendly(this DataTable dt)
    {
        Requires.NonNull(dt, nameof(dt));

        foreach (DataColumn dc in dt.Columns)
        {
            if (dc.ColumnName.Length > MaxTableColumnNameLength)
            {
                dc.ColumnName = dc.ColumnName.TruncateWithMidlineEllipsis(MaxTableColumnNameLength);
            }
        }
    }
}
