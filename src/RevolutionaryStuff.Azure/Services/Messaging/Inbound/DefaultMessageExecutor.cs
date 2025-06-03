using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;

internal class DefaultMessageExecutor(
    IOptions<DefaultMessageExecutor.Config> ConfigOptions,
    ILogger<DefaultMessageExecutor> logger)
    : BaseLoggingDisposable(logger), IDefaultMessageExecutor
{
    public class Config
    {
        public const string ConfigSectionName = "InboundMessageExecutor";

        public string MessagePropertiesPrefix { get; set; } = "message.";

        public bool LogMessageProperties { get; set; } = true;
    }

    private static class WellKnownPropertyKeys
    {
        public const string MessageId = "MessageId";
        public const string ContentType = "ContentType";
        public const string SequenceNumber = "SequenceNumber";
        public const string CorrelationId = "CorrelationId";
        public const string EnqueuedTime = "EnqueuedTime";
        public const string Subject = "Subject";

        public static readonly string[] All =
        [
            MessageId, ContentType, SequenceNumber, CorrelationId, EnqueuedTime, Subject
        ];
    }

    async Task IInboundMessageExecutor.ExecuteAsync(IInboundMessage message, Func<IInboundMessage, Task> processAsync)
    {
        var config = ConfigOptions.Value;
        if (config.LogMessageProperties)
        {
            RegisterDisposableObject(LogScopedProperty($"{config.MessagePropertiesPrefix}{WellKnownPropertyKeys.MessageId}", message.MessageId));
            RegisterDisposableObject(LogScopedProperty($"{config.MessagePropertiesPrefix}{WellKnownPropertyKeys.ContentType}", message.ContentType));
            RegisterDisposableObject(LogScopedProperty($"{config.MessagePropertiesPrefix}{WellKnownPropertyKeys.SequenceNumber}", message.SequenceNumber));
            RegisterDisposableObject(LogScopedProperty($"{config.MessagePropertiesPrefix}{WellKnownPropertyKeys.CorrelationId}", message.CorrelationId));
            RegisterDisposableObject(LogScopedProperty($"{config.MessagePropertiesPrefix}{WellKnownPropertyKeys.EnqueuedTime}", message.EnqueuedTime));
            RegisterDisposableObject(LogScopedProperty($"{config.MessagePropertiesPrefix}{WellKnownPropertyKeys.Subject}", message.Subject));
            foreach (var kvp in message.Properties.NullSafeEnumerable().Where(z => !WellKnownPropertyKeys.All.Contains(z.Key)))
            {
                RegisterDisposableObject(LogScopedProperty($"{config.MessagePropertiesPrefix}{kvp.Key}", kvp.Value, decomposeValue: true));
            }
        }
        await processAsync(message);
    }
}

