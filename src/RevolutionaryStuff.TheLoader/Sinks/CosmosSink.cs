using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Threading.Tasks;
//using Azure.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.TheLoader.Sinks;

namespace RevolutionaryStuff.TheLoader.Uploaders
{
    public class CosmosSink : BaseSink
    {
        public const string PARTITION_KEY_FIELD_NAME = "partitionKey";

        public class AuthenticationConfig
        { 
            public string ConnectionString { get; set; }
            public string DatabaseName { get; set; }
        }

        private readonly AuthenticationConfig AConfig;

        public CosmosSink(AuthenticationConfig authenticationConfig, Program program, IOptions<Program.LoaderConfig.TableConfig> tableConfigOptions)
            : base(program, tableConfigOptions)
        {
            AConfig = authenticationConfig;
            MaxConcurrentAsyncTasks = program.Parallelism ? 50 : 1;
        }

        int MaxConcurrentAsyncTasks = 1;

        private CosmosClientOptions GetCosmosClientOptions()
        {
            var cco = new CosmosClientOptions();
            cco.AllowBulkExecution = true;
            cco.ConnectionMode = ConnectionMode.Gateway;
//            cco.Retry.MaxRetries = 5;
//            cco.Retry.Mode = Azure.Core.RetryMode.Fixed;
//            cco.Retry.Delay = TimeSpan.FromMilliseconds(100);
            return cco;
        }

        protected override async Task OnUploadAsync(DataTable dt)
        {
            var tableConfig = TableConfigOptions.Value;
            var partitionKeyFieldName = tableConfig.PartitionKeyFieldName ?? PARTITION_KEY_FIELD_NAME;
            var missingPartitionKeyValue = $"{DateTime.UtcNow.ToRfc8601()}-{Environment.MachineName}";
            using (var client = new CosmosClient(AConfig.ConnectionString, GetCosmosClientOptions()))
            {
                var databaseResp = await client.CreateDatabaseIfNotExistsAsync(AConfig.DatabaseName);
                var containerResp = await databaseResp.Database.CreateContainerIfNotExistsAsync(new ContainerProperties { Id = dt.TableName, PartitionKeyPath = "/"+ partitionKeyFieldName });
                var container = containerResp.Container;
                await TaskHelpers.TaskWhenAllForEachAsync(dt.Rows.ToList(), dr => {
                    var d = new ExpandoObject();
                    var items = dr.ItemArray;
                    PartitionKey pk = default;
                    bool pkFound = true;
                    for (int z = 0; z < dt.Columns.Count; ++z)
                    {
                        var val = items[z];
                        if (val == DBNull.Value || val == null) continue;
                        var c = dt.Columns[z];
                        if (c.ColumnName == "id")
                        {
                            val = val.ToString();
                        }
                        if (!d.TryAdd(c.ColumnName, val))
                        {
                            throw new Exception($"Could not add column {z} [{c.ColumnName}]");
                        }
                        if (c.ColumnName == partitionKeyFieldName)
                        {
                            pk = new PartitionKey(Stuff.ObjectToString(val));
                            pkFound = true;
                        }
                    }
                    if (!pkFound)
                    {
                        d.TryAdd(partitionKeyFieldName, missingPartitionKeyValue);
                        pk = new PartitionKey(missingPartitionKeyValue);
                    }
                    return container.UpsertItemAsync<object>(d, pk);
                }, MaxConcurrentAsyncTasks);
            }
        }
    }
}
