using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections;
using System.Data;

namespace RevolutionaryStuff.Core.Database
{
    public static class ConnectionHelpers
    {
        public static TimeSpan DefaultTimeout = TimeSpan.FromMinutes(1);

        public static void OpenIfNeeded(this IDbConnection conn)
        {
            Requires.NonNull(conn, nameof(conn));
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        public static bool TableExists(this IDbConnection conn, string tableName, string schemaName = null)
        {
            tableName = StringHelpers.TrimOrNull(tableName);
            if (tableName == null) return false;
            schemaName = SqlHelpers.SchemaOrDefault(schemaName);
            var exists = conn.ExecuteScalar<object>($"select 1 from information_schema.tables where table_name='{tableName.EscapeForSql()}' and table_schema='{schemaName.EscapeForSql()}'");
            return 1.CompareTo(exists) == 0;
        }

        public static void ExecuteNonQuery(this IDbTransaction trans, string sql)
        {
            trans.Connection.ExecuteNonQuery(sql, trans);
        }

        public static void ExecuteNonQuery(this IDbConnection conn, string sql, IDbTransaction trans = null, bool useNewTransaction = false, TimeSpan? timeout = null)
        {
            Requires.NonNull(conn, nameof(conn));
            Requires.Text(sql, nameof(sql));

            Debug.WriteLine(sql);
            if (useNewTransaction)
            {
                Requires.Null(trans, "t");
            }
            conn.OpenIfNeeded();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                if (timeout != null)
                {
                    cmd.CommandTimeout = Convert.ToInt32(timeout.Value.TotalSeconds);
                }
                if (useNewTransaction)
                {
                    using (var newTrans = conn.BeginTransaction())
                    {
                        cmd.Transaction = newTrans;
                        cmd.ExecuteNonQuery();
                        newTrans.Commit();
                    }
                }
                else
                {
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void BulkUpload(this SqlConnection conn, DataTable dt, string schema, string table, Action<long, long> notificationCallback = null, int notifyIncrement = 1000, TimeSpan? timeout = null)
        {
            Requires.NonNull(conn, nameof(conn));
            Requires.NonNull(dt, nameof(dt));
            Requires.Text(schema, nameof(schema));
            Requires.Text(table, nameof(table));

            var copy = new SqlBulkCopy(conn);
            if (timeout != null)
            {
                copy.BulkCopyTimeout = Convert.ToInt32(timeout.Value.TotalSeconds);
            }
            copy.DestinationTableName = string.Format("{0}.{1}", schema, table);
            copy.NotifyAfter = notifyIncrement;
            if (notificationCallback != null)
            {
                copy.SqlRowsCopied += (sender, e) => notificationCallback(e.RowsCopied, dt.Rows.Count);
            }
            foreach (DataColumn dc in dt.Columns)
            {
                copy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
            }
            copy.WriteToServer(dt.CreateDataReader());
            copy.Close();
        }

        public static void AddRange(this SqlParameterCollection pc, IEnumerable<SqlParameter> parameters)
        {
            if (parameters == null) return;
            foreach (var p in parameters)
            {
                pc.Add(p);
            }
        }

        public static Result ExecuteNonQuery(
            this IDbConnection conn,
            string sql,
            params SqlParameter[] parameters)
            => conn.ExecuteNonQuery(null, sql, null, parameters);

        public static Result ExecuteNonQuery(
            this IDbConnection conn,
            IDbTransaction trans,
            string sql,
            TimeSpan? timeout,
            IEnumerable<SqlParameter> parameters
            )
        {
            Requires.NonNull(conn, nameof(conn));
            Requires.Text(sql, nameof(sql));
            conn.OpenIfNeeded();
            using (var cmd = new SqlCommand(sql, (SqlConnection)conn)
            {
                Transaction = (SqlTransaction)trans,
                CommandTimeout = Convert.ToInt32(timeout.GetValueOrDefault(DefaultTimeout).TotalSeconds)
            })
            {
                cmd.Parameters.AddRange(parameters);
                cmd.CommandType = cmd.Parameters.Count == 0 ? CommandType.Text : CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                return new Result(cmd.Parameters);
            }
        }

        public static T ExecuteScalar<T>(
            this IDbConnection conn,
            string sql,
            TimeSpan? timeout = null,
            IEnumerable<SqlParameter> parameters = null)
            => (T)conn.ExecuteScalar(null, sql, timeout, parameters);

        public static object ExecuteScalar(
            this IDbConnection conn,
            IDbTransaction trans,
            string sql,
            TimeSpan? timeout=null,
            IEnumerable<SqlParameter> parameters=null)
        {
            Requires.NonNull(conn, nameof(conn));
            Requires.Text(sql, nameof(sql));
            using (var cmd = new SqlCommand(sql, (SqlConnection)conn)
            {
                Transaction = (SqlTransaction)trans,
                CommandTimeout = Convert.ToInt32(timeout.GetValueOrDefault(DefaultTimeout).TotalSeconds)
            })
            {
                cmd.Parameters.AddRange(parameters);
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                OpenIfNeeded(conn);
                return cmd.ExecuteScalar();
            }
        }

        public async static Task<Result> ExecuteNonQueryAsync(
            this IDbConnection conn,
            IDbTransaction trans,
            string sql,
            TimeSpan? timeout,
            IEnumerable<SqlParameter> parameters,
            CommandType? forcedCommandType=null
            )
        {
            Requires.NonNull(conn, nameof(conn));
            Requires.Text(sql, nameof(sql));
            conn.OpenIfNeeded();
            using (var cmd = new SqlCommand(sql, (SqlConnection)conn)
            {
                Transaction = (SqlTransaction)trans,
                CommandTimeout = Convert.ToInt32(timeout.GetValueOrDefault(DefaultTimeout).TotalSeconds)
            })
            {
                cmd.Parameters.AddRange(parameters);
                cmd.CommandType = forcedCommandType.GetValueOrDefault(cmd.Parameters.Count == 0 ? CommandType.Text : CommandType.StoredProcedure);
                await cmd.ExecuteNonQueryAsync();
                return new Result(cmd.Parameters);
            }
        }

        public static Result ExecuteReaderForEach(
            this IDbConnection conn,
            IDbTransaction trans,
            string sql,
            TimeSpan? timeout,
            IEnumerable<SqlParameter> parameters,
            params Action<IDataReader>[] actions)
        {
            return conn.ExecuteReaderForEachAsync(trans, sql, timeout, parameters, actions).ExecuteSynchronously();
        }

        public async static Task<Result> ExecuteReaderForEachAsync(
            this IDbConnection conn,
            IDbTransaction trans,
            string sql,
            TimeSpan? timeout,
            IEnumerable<SqlParameter> parameters,
            params Action<IDataReader>[] actions)
        {
            Requires.NonNull(conn, nameof(conn));
            Requires.Text(sql, nameof(sql));
            conn.OpenIfNeeded();
            try
            {
                using (var cmd = new SqlCommand(sql, (SqlConnection)conn)
                {
                    Transaction = (SqlTransaction)trans,
                    CommandTimeout = Convert.ToInt32(timeout.GetValueOrDefault(DefaultTimeout).TotalSeconds)
                })
                {
                    cmd.Parameters.AddRange(parameters);
                    cmd.CommandType = cmd.Parameters.Count == 0 ? CommandType.Text : CommandType.StoredProcedure;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        foreach (var action in actions)
                        {
                            while (await reader.ReadAsync())
                            {
                                action(reader);
                            }
                            await reader.NextResultAsync();
                        }
                    }
                    return new Result(cmd.Parameters);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        public async static Task<Result<TItem>> ExecuteReaderAsync<TItem>(
            this IDbConnection conn,
            IDbTransaction trans,
            string sql,
            TimeSpan? timeout,
            IEnumerable<SqlParameter> parameters) where TItem : new()
        {
            var items = new List<TItem>();
            var map = GetSettersByNameMap<TItem>();
            var res = await conn.ExecuteReaderForEachAsync(trans, sql, timeout, parameters, r =>
            {
                var item = r.Get<TItem>(map);
                items.Add(item);
            });
            return new Result<TItem>(res, items);
        }

        private static IDictionary<string, MemberInfo> GetSettersByNameMap<TItem>()
        {
            var t = typeof(TItem);
            return Cache.DataCacher.FindOrCreateValue(
                Cache.CreateKey(typeof(ConnectionHelpers), nameof(GetSettersByNameMap), t),
                () =>
                {
                    var d = new Dictionary<string, MemberInfo>();
                    foreach (var mi in t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
                    {
                        var pi = mi as PropertyInfo;
                        if (pi == null) continue;
                        if (pi.Attributes.HasFlag(System.Reflection.PropertyAttributes.SpecialName | System.Reflection.PropertyAttributes.RTSpecialName)) continue;
                        var colAttr = mi.GetCustomAttribute<ColumnAttribute>();
                        d[colAttr?.Name ?? mi.Name] = mi;
                    }
                    return d;
                });
        }

        public static TItem Get<TItem>(this IDataReader reader, IDictionary<string, MemberInfo> map = null) where TItem : new()
        {
            map = map ?? GetSettersByNameMap<TItem>();
            if (map.Count == 0)
            {
                var val = reader[0];
                if (val.GetType() == typeof(TItem))
                {
                    return (TItem) val;
                }
            }
            var item = new TItem();
            var fieldsToSkip = new HashSet<string>();
            foreach (var kvp in map)
            {
                var fieldName = kvp.Key;
                if (fieldsToSkip.Contains(fieldName)) continue;
                object val;
                try
                {
                    val = reader[fieldName];
                }
                catch (IndexOutOfRangeException)
                {
                    fieldsToSkip.Add(fieldName);
                    continue;
                }
                if (val == DBNull.Value)
                {
                    val = null;
                }
                kvp.Value.SetValue(item, val);
            }
            return item;
        }

        public class Result<TItem> : Result, IEnumerable<TItem>
        {
            public IList<TItem> Items { get; }

            internal Result(Result r, IList<TItem> items)
                : base(r)
            {
                Items = items;
            }

            public IEnumerator<TItem> GetEnumerator() => Items.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

            public override string ToString() => $"{base.ToString()} cnt={Items.Count}";
        }

        public class Result
        {
            public int? ReturnValue { get; private set; }
            private readonly SqlParameterCollection Parameters;

            #region Constructors

            public Result(Result other)
                : this(other.Parameters)
            { }

            public Result(SqlParameterCollection parameters)
            {
                Parameters = parameters;
            }

            public Result(int? returnValue, SqlParameterCollection parameters)
                : this(parameters)
            {
                ReturnValue = returnValue;
            }

            #endregion

            public override string ToString() => $"{this.GetType().Name} ret={ReturnValue}";

            public T GetOutputParameterVal<T>()
            {

                foreach (SqlParameter p in Parameters)
                {
                    if (p.Direction.HasFlag(ParameterDirection.Output))
                    {
                        return (T)p.Value;
                    }
                }
                throw new ArgumentException("There were no output parameters");
            }

            public T GetOutputParameterVal<T>(string name)
            {
                if (name.StartsWith("@"))
                {
                    name = name.Substring(1);
                }
                foreach (SqlParameter p in Parameters)
                {
                    var pn = p.ParameterName;
                    if (pn.StartsWith("@"))
                    {
                        pn = pn.Substring(1);
                    }
                    if (0 == string.Compare(pn, name, true) && p.Direction.HasFlag(ParameterDirection.Output))
                    {
                        if (p.Value == DBNull.Value)
                        {
                            return default;
                        }
                        return (T)p.Value;
                    }
                }
                throw new ArgumentException(string.Format("{0} was not in the parameter set for the sproc", name), "name");
            }

            public T GetOutputParameterVal<T>(int position)
            {
                var p = Parameters[position];
                if (p.Direction.HasFlag(ParameterDirection.Output))
                {
                    return (T)p.Value;
                }
                throw new ArgumentException(string.Format("{0} was not in the parameter set for the sproc", position), "position");
            }

            public object GetOutputParameterVal(int position)
            {
                return GetOutputParameterVal<object>(position);
            }

            public object GetOutputParameterVal(string name)
            {
                return GetOutputParameterVal<object>(name);
            }
        }
    }
}
