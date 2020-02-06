using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using Azure.Cosmos;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.TheLoader.Uploaders
{
    public class CosmosUploader : BaseUploader
    {
        public class AuthenticationConfig
        { 
            public string ConnectionString { get; set; }
            public string DatabaseName { get; set; }

            public string ParitionKeyFieldName { get; set; } = "_run";

            public string ParititionKeyValue { get; set; } = DateTime.UtcNow.ToRfc8601();
        }

        private readonly AuthenticationConfig AConfig;

        public CosmosUploader(Program program, AuthenticationConfig authenticationConfig)
            : base(program)
        {
            AConfig = authenticationConfig;
        }

        private CosmosClientOptions GetOptions()
        {
            var cco = new CosmosClientOptions();
            cco.Retry.MaxRetries = 5;
            cco.Retry.Mode = Azure.Core.RetryMode.Fixed;
            cco.Retry.Delay = TimeSpan.FromMilliseconds(100);
            return cco;
        }

        protected override async Task OnUploadAsync(DataTable dt)
        {
            using (var client = new CosmosClient(AConfig.ConnectionString, GetOptions()))
            {
                var databaseResp = await client.CreateDatabaseIfNotExistsAsync(AConfig.DatabaseName);
                var containerResp = await databaseResp.Database.CreateContainerIfNotExistsAsync(new ContainerProperties { Id = dt.TableName, PartitionKeyPath = "/"+AConfig.ParitionKeyFieldName });
                var container = containerResp.Container;
                var pk = new PartitionKey(AConfig.ParititionKeyValue);
                foreach (DataRow dr in dt.Rows)
                {
                    var d = new ExpandoObject();
                    d.TryAdd(AConfig.ParitionKeyFieldName, AConfig.ParititionKeyValue);
                    var items = dr.ItemArray;
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
                    }
                    var resp = await container.CreateItemAsync<object>(d, pk);
                    Stuff.Noop(resp);
                }
            }
        }
    }
}
