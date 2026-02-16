using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Azure.Services.Authentication;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;
using RevolutionaryStuff.Core.Services.Tenant;
using RevolutionaryStuff.Data.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public abstract class CosmosJsonEntityServer : BaseLoggingDisposable, IJsonEntityServer
{
    private static readonly IDictionary<string, CosmosClient> CosmosClientByConnectionString = new ConcurrentDictionary<string, CosmosClient>();
    private readonly IAzureTokenCredentialProvider AzureTokenCredentialProvider;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IOptions<CosmosJsonEntityServerConfig> ConfigOptions;
    private readonly IDictionary<string, IJsonEntityContainer> JsonEntityContainerByRepositoryId = new ConcurrentDictionary<string, IJsonEntityContainer>();

    protected CosmosJsonEntityServer(CosmosJsonEntityServerConstructorArgs constructorArgs, ILogger logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(constructorArgs);

        AzureTokenCredentialProvider = constructorArgs.AzureTokenCredentialProvider;
        ServiceProvider = constructorArgs.ServiceProvider;
        ConfigOptions = constructorArgs.ConfigOptions;
    }

    /// <summary>
    /// This should take into account the current TenantId
    /// </summary>
    /// <returns>The connection string for the underlying CosmosClient</returns>
    protected abstract string GetConnectionString(string connectionStringName);

    /// <summary>
    /// This should take into account the current TenantId
    /// </summary>
    /// <returns>CosmosClientOptions associated with the to be created CosmosClient</returns>
    private CosmosClientOptions CreateCosmosClientOptions()
    {
        var cco = new CosmosClientOptions();
        ConfigureCosmosClientOptions(cco);
        return cco;
    }

    protected class IgnoreDataTypePropertyModifier : IJsonTypeInfoResolver
    {
        private readonly IJsonTypeInfoResolver InnerResolver;

        public IgnoreDataTypePropertyModifier(IJsonTypeInfoResolver innerResolver)
        {
            InnerResolver = innerResolver;
        }

        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var typeInfo = InnerResolver.GetTypeInfo(type, options);

            if (typeInfo != null)
            {
                var hasPolymorphicAttribute = type.GetCustomAttribute<JsonPolymorphicAttribute>(true) != null;
                if (hasPolymorphicAttribute)
                {
                    foreach (var property in typeInfo.Properties.ToList())
                    {
                        var propertyOptions = (property.AttributeProvider?.GetCustomAttributes(typeof(JsonTypeInfoResolverOptionsAttribute), true)?.FirstOrDefault()) as JsonTypeInfoResolverOptionsAttribute;
                        if (propertyOptions?.RemoveWhenPolymorphic ?? false)
                        {
                            typeInfo.Properties.Remove(property);
                        }
                    }
                }
            }

            return typeInfo;
        }
    }

    protected virtual void ConfigureCosmosClientOptions(CosmosClientOptions cosmosClientOptions)
    {
        var config = ConfigOptions.Value;
        DefaultJsonTypeInfoResolver resolver = new();
        JsonSerializerOptions jso = new(SystemTextJsonSerializer.DefaultJsonSerializationSettings)
        {
            TypeInfoResolver = new IgnoreDataTypePropertyModifier(resolver)
        };
        cosmosClientOptions.Serializer = new DefaultCosmosEntitySerializer(new SystemTextJsonSerializer(jso));
        cosmosClientOptions.ConnectionMode = config.ConnectionMode;
        cosmosClientOptions.ApplicationName = config.ApplicationName ?? RevolutionaryStuffCoreConfig.GetApplicationName(ServiceProvider.GetRequiredService<IConfiguration>());
    }

    protected abstract IJsonEntityContainer CreateJsonEntityContainer(Container container);

    private CosmosClient CosmosClient
    {
        get
        {
            if (field == null)
            {
                var config = ConfigOptions.Value;
                var connectionString = GetConnectionString(config.ConnectionStringName);
                if (!CosmosClientByConnectionString.TryGetValue(connectionString, out field))
                {
                    lock (CosmosClientByConnectionString)
                    {
                        field = CosmosClientByConnectionString.FindOrCreate(
                            connectionString,
                            () => ConstructCosmosClient(connectionString, CreateCosmosClientOptions())
                            );
                    }
                }
            }
            return field;
        }
    }

    protected virtual CosmosClient ConstructCosmosClient(string connectionString, CosmosClientOptions cosmosClientOptions)
        => CosmosHelpers.ConstructCosmosClient(
            new CosmosHelpers.CosmosClientAuthenticationSettings(connectionString, AzureTokenCredentialProvider, ConfigOptions.Value.AuthenticateWithWithDefaultAzureCredentials, true),
            cosmosClientOptions);

    /// <summary>
    /// You may want to override this if you want to do things such as changing the database based on the Tenant
    /// </summary>
    /// <param name="containerKey">The single string key for a container which ultimately maps its config</param>
    /// <returns>The container config</returns>
    protected virtual CosmosJsonEntityServerConfig.ContainerConfig GetContainerConfig(string containerKey)
        => ConfigOptions.Value.ContainerConfigByContainerKey?.GetValue(containerKey);

    protected virtual string GetTenantId()
        => null;

    IJsonEntityContainer IJsonEntityServer.GetContainer(string containerId)
    {
        Requires.Text(containerId);
        return JsonEntityContainerByRepositoryId.FindOrCreate(containerId, () =>
        {

            var containerInfo = GetContainerConfig(containerId);
            ArgumentNullException.ThrowIfNull(containerInfo, $"Cannot find containerInfo for containerKey=[{containerId}]");
            var container = CosmosClient.GetContainer(containerInfo.DatabaseConfig.DatabaseId, containerInfo.ContainerId);
            var jec = CreateJsonEntityContainer(container);
            if (jec is ITenantIdHolder tenantIdHolder)
            {
                var tenantId = GetTenantId();
                if (tenantId != null)
                {
                    tenantIdHolder.TenantId = tenantId;
                }
            }
            return jec;
        });
    }
}
