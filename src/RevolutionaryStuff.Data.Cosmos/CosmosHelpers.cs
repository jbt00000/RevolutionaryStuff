using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.Threading;
//using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Azure.Cosmos;
//using Microsoft.Azure.Cosmos.Encryption;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Scripts;
using RevolutionaryStuff.Azure.Services.Authentication;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.Cosmos;

public static class CosmosHelpers
{
    public class CosmosClientAuthenticationSettings : IValidate
    {
        public string CosmosClientConnectionStringOrEndpoint { get; }
        public IAzureTokenCredentialProvider AzureTokenCredentialProvider { get; set; }
        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;
        public bool WithEncryption { get; set; } = false;

        public CosmosClientAuthenticationSettings(string cosmosClientConnectionStringOrEndpoint, IAzureTokenCredentialProvider azureTokenCredentialProvider, bool? authenticateWithWithDefaultAzureCredentials = null, bool withEncryption = false)
        {
            Requires.Text(cosmosClientConnectionStringOrEndpoint);
            CosmosClientConnectionStringOrEndpoint = cosmosClientConnectionStringOrEndpoint;
            AzureTokenCredentialProvider = azureTokenCredentialProvider;
            AuthenticateWithWithDefaultAzureCredentials = authenticateWithWithDefaultAzureCredentials ?? (AzureTokenCredentialProvider != null);
            WithEncryption = withEncryption;
        }

        public void Validate()
            => ExceptionHelpers.AggregateExceptionsAndReThrow(
                () => Requires.Text(CosmosClientConnectionStringOrEndpoint),
                () => Requires.True(!AuthenticateWithWithDefaultAzureCredentials || AzureTokenCredentialProvider != null),
                () => { if (WithEncryption) Requires.True(AuthenticateWithWithDefaultAzureCredentials); }
                );
    }

    public class TestItem
    {
        [JsonPropertyName("sk")]
        public string PartitionKey { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("secd")]
        public string SecureDeterministic { get; set; }

        [JsonPropertyName("secr")]
        public string SecureRandom { get; set; }
    }

    private static async Task CreateTestItemAsync(Container c, TestItem item)
    {
        var pk = new PartitionKey(item.PartitionKey);
        var resp = await c.CreateItemAsync(item, pk);
        var read = (await c.ReadItemAsync<TestItem>(item.Id, pk)).Resource;
        System.Diagnostics.Trace.WriteLine($"Created item {item.Id} with pk {item.PartitionKey} {resp.RequestCharge} and read it back {read.SecureDeterministic} {read.SecureRandom}");
    }

    private static async Task PopulateContainerAsync(CosmosClient client, string dbName, string containerName)
    {
        try
        {
            var database = client.GetDatabase(dbName);
            var container = database.GetContainer(containerName);
            await CreateTestItemAsync(container, new TestItem { Id = "1", PartitionKey = "1", SecureDeterministic = "1", SecureRandom = "1" });
            await CreateTestItemAsync(container, new TestItem { Id = "2", PartitionKey = "1", SecureDeterministic = "1", SecureRandom = "1" });
            await CreateTestItemAsync(container, new TestItem { Id = "3", PartitionKey = "1", SecureDeterministic = "1", SecureRandom = "1" });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex.ToString());
            throw;
        }
    }

#if false
    private static async Task SetupEncryptedDatabaseAsync(CosmosClient client, string dbName)
    {
        try
        {
            await PopulateContainerAsync(client, "shuffletron", "internal");




            var dbKeyName = "dbKey01";
            await client.CreateDatabaseIfNotExistsAsync(dbName);
            var database = client.GetDatabase(dbName);
            //            var keyId = "https://kv-eu2-dev-shuffletron.vault.azure.net/keys/cos-sql-cmk-02/bcaab797fe0a412eb25f39ab47ba289a";
            if (Environment.TickCount >= 1) throw new NotImplementedException("HARDCODED PASSWORD HERE - probably for testing always encrypted");
            var keyId = "https://kv-eu2-dev-shuffletron.vault.azure.net/keys/cosSqlCmk/3f63134663274c55a1253eefeff1b33a";
            await database.CreateClientEncryptionKeyAsync(
                dbKeyName,
                DataEncryptionAlgorithm.AeadAes256CbcHmacSha256,
                new EncryptionKeyWrapMetadata(
                    KeyEncryptionKeyResolverName.AzureKeyVault,
                    "akvKey",
                    keyId,
                    EncryptionAlgorithm.RsaOaep.ToString()));

            var secureDeterministic = new ClientEncryptionIncludedPath
            {
                Path = "/secd",
                ClientEncryptionKeyId = dbKeyName,
                EncryptionType = EncryptionType.Deterministic.ToString(),
                EncryptionAlgorithm = DataEncryptionAlgorithm.AeadAes256CbcHmacSha256
            };
            var secureRandom = new ClientEncryptionIncludedPath
            {
                Path = "/secr",
                ClientEncryptionKeyId = dbKeyName,
                EncryptionType = EncryptionType.Randomized.ToString(),
                EncryptionAlgorithm = DataEncryptionAlgorithm.AeadAes256CbcHmacSha256
            };
            await database.DefineContainer("e", "/sk")
                .WithClientEncryptionPolicy()
                .WithIncludedPath(secureDeterministic)
                .WithIncludedPath(secureRandom)
                .Attach()
                .CreateAsync();
            await PopulateContainerAsync(client, dbName, "e");

        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex.ToString());
            throw;
        }
    }
#endif

    public static CosmosClient ConstructCosmosClient(CosmosClientAuthenticationSettings authenticationSettings, CosmosClientOptions cosmosClientOptions)
    {
        Requires.Valid(authenticationSettings);
        ArgumentNullException.ThrowIfNull(cosmosClientOptions);

        CosmosClient client;

        if (authenticationSettings.AuthenticateWithWithDefaultAzureCredentials)
        {
            var creds = authenticationSettings.AzureTokenCredentialProvider.GetTokenCredential();
            client = new CosmosClient(authenticationSettings.CosmosClientConnectionStringOrEndpoint, creds, cosmosClientOptions);
            //client = new CosmosClient("<real connection string with account keys....>", cosmosClientOptions);
            if (authenticationSettings.WithEncryption)
            {
                //TODO: client.WithEncryption seems to NOT work with unencrypted containers... I think this may be due to mismatch/old cosmos vs cosmos encrypted packages
                //TODO: continue to figure out how to setup encrypted cosmos databases in bicep, the SetupEncryptedDatabaseAsync method does this in code, IFF you are using a real (as opposed to rbac) connection string
                //                client = client.WithEncryption(new KeyResolver(creds), KeyEncryptionKeyResolverName.AzureKeyVault);
                //                            SetupEncryptedDatabaseAsync(client, "edb5").ExecuteSynchronously();
            }


        }
        else
        {
            client = new CosmosClient(authenticationSettings.CosmosClientConnectionStringOrEndpoint, cosmosClientOptions);
        }
        return client;
    }

    extension<T>(IQueryable<T> sources) where T : class
    {
        public async Task<T?> GetFirstOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sources);

            try
            {
                using var fi = sources.Take(1).ToFeedIterator();
                ArgumentNullException.ThrowIfNull(fi);
                while (fi.HasMoreResults)
                {
                    var resp = await fi.ReadNextAsync(cancellationToken);
                    //at least we can set a breakpoint here to look at items inside of the FeedResponse like cost!
                    return resp.FirstOrDefault();
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This is expected and suppressed
            }
            return default;
        }
        public async Task<IReadOnlyList<T>> GetAllItemsAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sources);


            if (sources is IList<T> list)
            {
                return list.AsReadOnly();
            }
            else if (sources is IAsyncEnumerable<T> ae)
            {
                List<T> items = [];
                await foreach (var item in ae.WithCancellation(cancellationToken))
                {
                    items.Add(item);
                }
                return items.AsReadOnly();
            }
            else
            {
                List<T> items = [];
                try
                {
                    using var fi = sources.ToFeedIterator();
                    ArgumentNullException.ThrowIfNull(fi);
                    while (fi.HasMoreResults)
                    {
                        var resp = await fi.ReadNextAsync(cancellationToken);
                        //at least we can set a breakpoint here to look at items inside of the FeedResponse like cost!
                        items.AddRange(resp);
                    }
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Stuff.NoOp();
                    // This is expected and suppressed
                }
                catch (System.ArgumentOutOfRangeException ex) when (ex.ParamName == "linqQuery") //yep... through decompilation
                {
                    items = sources.ToList();
                }
                return items.AsReadOnly();
            }
        }
    }

    extension<T>(IQueryable<T> sources)
    {
        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sources);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var response = await CosmosLinqExtensions.CountAsync(sources, cancellationToken);
                return response.Resource;
            }
            catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "linqQuery")
            {
                cancellationToken.ThrowIfCancellationRequested();
                return sources.Count();
            }
        }
        public Task<int> GetSumAsync(Expression<Func<T, int>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<int?> GetSumAsync(Expression<Func<T, int?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<long> GetSumAsync(Expression<Func<T, long>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<long?> GetSumAsync(Expression<Func<T, long?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<float> GetSumAsync(Expression<Func<T, float>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<float?> GetSumAsync(Expression<Func<T, float?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<double> GetSumAsync(Expression<Func<T, double>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<double?> GetSumAsync(Expression<Func<T, double?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<decimal> GetSumAsync(Expression<Func<T, decimal>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<decimal?> GetSumAsync(Expression<Func<T, decimal?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.SumAsync(selected, ct), (source, expression) => source.Sum(expression), cancellationToken);
        public Task<double> GetAverageAsync(Expression<Func<T, int>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<double?> GetAverageAsync(Expression<Func<T, int?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<double> GetAverageAsync(Expression<Func<T, long>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<double?> GetAverageAsync(Expression<Func<T, long?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<float> GetAverageAsync(Expression<Func<T, float>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<float?> GetAverageAsync(Expression<Func<T, float?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<double> GetAverageAsync(Expression<Func<T, double>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<double?> GetAverageAsync(Expression<Func<T, double?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<decimal> GetAverageAsync(Expression<Func<T, decimal>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<decimal?> GetAverageAsync(Expression<Func<T, decimal?>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.AverageAsync(selected, ct), (source, expression) => source.Average(expression), cancellationToken);
        public Task<TValue> GetMinAsync<TValue>(Expression<Func<T, TValue>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.MinAsync(selected, ct), (source, expression) => source.Min(expression), cancellationToken);
        public Task<TValue> GetMaxAsync<TValue>(Expression<Func<T, TValue>> selector, CancellationToken cancellationToken = default)
                => ExecuteAggregateAsync(sources, selector, (selected, ct) => CosmosLinqExtensions.MaxAsync(selected, ct), (source, expression) => source.Max(expression), cancellationToken);
        public async Task<int> ExecuteForEachAsync(Func<T, Task> executeAsync, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sources);
            ArgumentNullException.ThrowIfNull(executeAsync);

            var count = 0;
            try
            {
                using var fi = sources.ToFeedIterator();
                ArgumentNullException.ThrowIfNull(fi);
                while (fi.HasMoreResults)
                {
                    var resp = await fi.ReadNextAsync(cancellationToken);
                    foreach (var item in resp)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        await executeAsync(item);
                        ++count;
                    }
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This is expected and suppressed
            }
            return count;
        }
    }













































    private static async Task<TResult> ExecuteAggregateAsync<T, TValue, TResult>(
        IQueryable<T> q,
        Expression<Func<T, TValue>> selector,
        Func<IQueryable<TValue>, CancellationToken, Task<Response<TResult>>> cosmosAggregateAsync,
        Func<IQueryable<T>, Expression<Func<T, TValue>>, TResult> queryableAggregate,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);
        ArgumentNullException.ThrowIfNull(selector);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var response = await cosmosAggregateAsync(q.Select(selector), cancellationToken);
            return response.Resource;
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "linqQuery")
        {
            cancellationToken.ThrowIfCancellationRequested();
            return queryableAggregate(q, selector);
        }
    }





    extension(Scripts source)
    {
        public async Task<StoredProcedureResponse> UpsertStoredProcedureAsync(StoredProcedureProperties properties)
        {
            try
            {
                return await source.ReplaceStoredProcedureAsync(properties);
            }
            catch (Exception)
            {
                return await source.CreateStoredProcedureAsync(properties);
            }
        }
        public async Task<TriggerResponse> UpsertTriggerAsync(TriggerProperties properties)
        {
            try
            {
                return await source.ReplaceTriggerAsync(properties);
            }
            catch (Exception)
            {
                return await source.CreateTriggerAsync(properties);
            }
        }
    }



    private class MyContinuationToken
    {
        public int TotalCount { get; set; }
        public string? CosmosContinuationToken { get; set; }
        public static MyContinuationToken? Decode(string s)
        {
            if (s != null)
            {
                try
                {
                    s = s.DecodeBase64String();
                    var parts = CSV.ParseLine(s);
                    if (parts.Length == 2)
                    {
                        return new()
                        {
                            TotalCount = int.Parse(parts[0]),
                            CosmosContinuationToken = parts[1]
                        };
                    }
                }
                catch (Exception) { }
            }
            return null;
        }

        public string GetToken()
            => CSV.FormatLine(new[] { TotalCount.ToString(), CosmosContinuationToken }, false).ToBase64String();
    }

    extension(Container source)
    {
        public Task<ItemResponse<T>> PatchItemAsync<T>(string id, string partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
                => source.PatchItemAsync<T>(id, new(partitionKey), patchOperations, requestOptions, cancellationToken);
    }
}
