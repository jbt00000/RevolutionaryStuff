using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevolutionaryStuff.Core.Database;

namespace RevolutionaryStuff.Core.Database
{
    public static class SqlServerHelpers
    {
        public const int MaxTableNameLength = 128;

        public static async Task TablePropertySetAsync(this SqlConnection conn, string tableName, string propertyName, object propertyValue, string schemaName = null)
        {
            schemaName = SqlHelpers.SchemaOrDefault(schemaName);
            var ps = new SqlParameter[]
                {
                    new SqlParameter("@propertyName", propertyName){Direction=ParameterDirection.Input},
                    new SqlParameter("@tableSchema", schemaName){Direction=ParameterDirection.Input},
                    new SqlParameter("@tableName", tableName){Direction=ParameterDirection.Input},
                    new SqlParameter("@cnt", SqlDbType.Int){Direction=ParameterDirection.Output},
                };
            var cnt = (await conn.ExecuteNonQueryAsync(null, "select @cnt=count(*) from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @tableSchema, N'TABLE', @tableName, null, null)", null, ps, CommandType.Text)).GetOutputParameterVal<int>();
            if (cnt > 0)
            {
                ps = new SqlParameter[]
                    {
                    new SqlParameter("@name", propertyName){Direction=ParameterDirection.Input},
                    new SqlParameter("@level0name", schemaName){Direction=ParameterDirection.Input},
                    new SqlParameter("@level1name", tableName){Direction=ParameterDirection.Input},
                    new SqlParameter("@level0type", "SCHEMA"){Direction=ParameterDirection.Input},
                    new SqlParameter("@level1type", "TABLE"){Direction=ParameterDirection.Input},
                    };
                await conn.ExecuteNonQueryAsync(null, "sys.sp_dropextendedproperty", null, ps);
            }
            ps = new SqlParameter[]
                {
                    new SqlParameter("@name", propertyName){Direction=ParameterDirection.Input},
                    new SqlParameter("@level0name", schemaName){Direction=ParameterDirection.Input},
                    new SqlParameter("@level1name", tableName){Direction=ParameterDirection.Input},
                    new SqlParameter("@level0type", "SCHEMA"){Direction=ParameterDirection.Input},
                    new SqlParameter("@level1type", "TABLE"){Direction=ParameterDirection.Input},
                    new SqlParameter("@value", propertyValue){Direction=ParameterDirection.Input},
                };
            await conn.ExecuteNonQueryAsync(null, "sys.sp_addextendedproperty", null, ps);
        }
    }
}
