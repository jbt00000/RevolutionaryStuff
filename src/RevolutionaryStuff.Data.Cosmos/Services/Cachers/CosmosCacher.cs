using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Azure.Services.Authentication;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Data.Cosmos.Services.Cachers;

public class CosmosCacher(IAzureTokenCredentialProvider AzureTokenCredentialProvider, IConnectionStringProvider ConnectionStringProvider, IOptions<CosmosCacher.Config> ConfigOptions) : BaseCacher, ICosmosCacher
{
    public class Config
    {
        public const string ConfigSectionName = "CosmosCacher";

        public TimeSpan? MaxCacheDuration { get; set; } = TimeSpan.FromDays(180);

        public string ConnectionStringName { get; set; }

        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;

        public string DatabaseName { get; set; }

        public string ContainerName { get; set; }
    }

    private Container Container
    {
        get
        {
            var config = ConfigOptions.Value;
            return PermaCache.FindOrCreate(
                Cache.CreateKey(config.DatabaseName, config.ContainerName, config.ContainerName, nameof(CosmosCacher)),
                () =>
                {
                    var connectionString = ConnectionStringProvider.GetConnectionString(config.ConnectionStringName);
                    var client = CosmosHelpers.ConstructCosmosClient(new(connectionString, AzureTokenCredentialProvider, config.AuthenticateWithWithDefaultAzureCredentials), new() { Serializer = new DefaultCosmosEntitySerializer() });
                    return client.GetDatabase(config.DatabaseName).GetContainer(config.ContainerName);
                });
        }
    }

    protected virtual string TransformKey(string key)
        => key;

    protected override async Task<CacheEntry> OnFindEntryAsync(string key)
    {
        key = TransformKey(key);
        var cosmosCacheEntity = await Container
            .GetItemLinqQueryable<CosmosCacheEntity>(false, null, new QueryRequestOptions() { PartitionKey = new(key) })
            .Where(z => z.Id == key)
            .GetFirstOrDefaultAsync();
        if (cosmosCacheEntity == null)
        {
            return null;
        }
        object val = null;
        if (cosmosCacheEntity.ClrTypeName != null)
        {
            var valType = Type.GetType(cosmosCacheEntity.ClrTypeName);
            var mi = typeof(JsonHelpers).GetMethod(nameof(JsonHelpers.FromJsonElement), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var gmi = mi.MakeGenericMethod(valType);
            val = gmi.Invoke(null, [cosmosCacheEntity.Value]);
        }
        var cacheEntry = new CacheEntry(val, cosmosCacheEntity.ExpiresAt.Subtract(cosmosCacheEntity.CreatedAt), cosmosCacheEntity.CreatedAt, cosmosCacheEntity.CreatedOn);
        return cacheEntry;
    }

    protected override Task OnWriteEntryAsync(string key, CacheEntry entry)
    {
        key = TransformKey(key);
        var val = entry.Value;
        var valType = val?.GetType();
        var cosmosCacheEntity = new CosmosCacheEntity()
        {
            Id = key,
            Ttl = Convert.ToInt32(Math.Ceiling(entry.ExpiresAt.Subtract(DateTimeOffset.UtcNow).TotalSeconds)),
            ClrTypeName = valType?.AssemblyQualifiedName,
            Value = JsonHelpers.ToJsonElement(val),
            CreatedAt = entry.CreatedAt,
            ExpiresAt = entry.ExpiresAt,
        };
        var json = JsonHelpers.ToJson(cosmosCacheEntity);
        System.Diagnostics.Trace.WriteLine(json);
        return Container.UpsertItemAsync(cosmosCacheEntity, new PartitionKey(key), new ItemRequestOptions() { EnableContentResponseOnWrite = false });
    }

    protected override Task OnRemoveAsync(string key)
    {
        key = TransformKey(key);
        return Container.DeleteItemAsync<CosmosCacheEntity>(key, new PartitionKey(key), new ItemRequestOptions() { EnableContentResponseOnWrite = false });
    }

    private class CosmosCacheEntity : CosmosEntity<string>
    {
        [JsonPropertyName(CosmosEntityPropertyNames.Ttl)]
        public int Ttl { get; set; }

        [JsonPropertyName("clrTypeName")]
        public string ClrTypeName { get; set; }

        [JsonPropertyName("value")]
        public JsonElement Value { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyName("expiresAt")]
        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyName("createdOn")]
        public string CreatedOn { get; set; } = Environment.MachineName;
    }
}
