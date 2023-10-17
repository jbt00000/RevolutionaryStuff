using System.Threading;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;

public abstract class ServiceBusMessageSender : BaseLoggingDisposable, IServiceBusMessageSender
{
    private readonly IConnectionStringProvider ConnectionStringProvider;
    private readonly IOptions<ServiceBusMessageSenderConfig> ConfigOptions;

    public class ServiceBusMessageSenderConfig
    {
        public const string ConfigSectionName = "ServiceBusMessageSenderConfig";
        public string ConnectionStringName { get; set; }
        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;
        public int MaxPayloadSize { get; set; } = 1024 * 256;
        public bool StoreOverMaxMessagesInDatalake { get; set; } = true;
    }

    public sealed class ServiceBusMessageSenderConstructorArgs
    {
        internal readonly IConnectionStringProvider ConnectionStringProvider;
        internal readonly IOptions<ServiceBusMessageSenderConfig> ConfigOptions;

        public ServiceBusMessageSenderConstructorArgs(IConnectionStringProvider connectionStringProvider, IOptions<ServiceBusMessageSenderConfig> configOptions)
        {
            ArgumentNullException.ThrowIfNull(connectionStringProvider);
            ArgumentNullException.ThrowIfNull(configOptions);

            ConnectionStringProvider = connectionStringProvider;
            ConfigOptions = configOptions;
        }
    }

    protected ServiceBusMessageSender(ServiceBusMessageSenderConstructorArgs constructorArgs, ILogger logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(constructorArgs);

        ConnectionStringProvider = constructorArgs.ConnectionStringProvider;
        ConfigOptions = constructorArgs.ConfigOptions;
    }

    private static readonly Dictionary<string, ServiceBusSender> ClientByKey = new();

    private ServiceBusSender GetClient(string messageContainerName)
    {
        Requires.Text(messageContainerName);

        var config = ConfigOptions.Value;
        var connectionString = ConnectionStringProvider.GetConnectionString(config.ConnectionStringName);
        var cacheKey = Cache.CreateKey(connectionString, messageContainerName);
        lock (typeof(ServiceBusSender))
        {
            return ClientByKey.FindOrCreate(
                cacheKey,
                () => ServiceBusHelpers.ConstructServiceBusClient(connectionString, config.AuthenticateWithWithDefaultAzureCredentials).CreateSender(messageContainerName));
        }
    }

    async Task IMessageSender.SendAsync(string port, OutboundMessage message, Action<IDictionary<string, object>> propertyOverride, MessageSendSettings settings, CancellationToken cancellationToken)
    {
        Requires.Text(port);

        var serviceBusMessage = await CreateServiceBusMessageAsync(message);

        if (settings != null)
        {
            if (settings.Properties.NullSafeAny())
            {
                settings.Properties.ForEach(kvp => serviceBusMessage.ApplicationProperties[kvp.Key] = kvp.Value);
            }
            if (settings.SendAt != null)
            {
                serviceBusMessage.ScheduledEnqueueTime = settings.SendAt.Value;
            }
            else if (settings.SendIn != null)
            {
                serviceBusMessage.ScheduledEnqueueTime = DateTimeOffset.UtcNow.Add(settings.SendIn.Value);
            }
        }

        propertyOverride?.Invoke(serviceBusMessage.ApplicationProperties);

        var client = GetClient(port);



        await client.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    protected async Task<ServiceBusMessage> CreateServiceBusMessageAsync(OutboundMessage outboundMessage)
    {
        Requires.Valid(outboundMessage);

        var config = ConfigOptions.Value;

        ServiceBusMessage serviceBusMessage;

        var size = outboundMessage.Size;

        serviceBusMessage = outboundMessage.Size >= config.MaxPayloadSize
            ? await OnCreateOversizeMessageAsync(outboundMessage)
            : new ServiceBusMessage(new BinaryData(await outboundMessage.Payload.ToBufferAsync()));

        await OnPopulateAdditionalPropertiesAsync(serviceBusMessage);

        serviceBusMessage.ContentType = outboundMessage.ContentType;

        return serviceBusMessage;
    }

    protected virtual Task OnPopulateAdditionalPropertiesAsync(ServiceBusMessage outboundMessage)
        => Task.CompletedTask;

    protected virtual Task<ServiceBusMessage> OnCreateOversizeMessageAsync(OutboundMessage outboundMessage)
        => throw new NotSupportedException("Message too large to send");
}
