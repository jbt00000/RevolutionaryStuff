using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;

internal class DefaultMessageExecutor(
    IOptions<DefaultMessageExecutor.Config> ConfigOptions,
    RevolutionaryStuffService.RevolutionaryStuffServiceConstrutorArgs BaseConstructorArgs)
    : RevolutionaryStuffService(BaseConstructorArgs), IDefaultMessageExecutor
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
        var scopes = new Stack<IDisposable>();

        try
        {
            var config = ConfigOptions.Value;
            if (config.LogMessageProperties)
            {
                void AddProperty(string key, object value, bool decomposeValue = false)
                {
                    var scope = LogScopedProperty(
                        $"{config.MessagePropertiesPrefix}{key}", value, decomposeValue);
                    if (scope != null)
                    {
                        scopes.Push(scope);
                    }
                }

                AddProperty(WellKnownPropertyKeys.MessageId, message.MessageId);
                AddProperty(WellKnownPropertyKeys.ContentType, message.ContentType);
                AddProperty(WellKnownPropertyKeys.SequenceNumber, message.SequenceNumber);
                AddProperty(WellKnownPropertyKeys.CorrelationId, message.CorrelationId);
                AddProperty(WellKnownPropertyKeys.EnqueuedTime, message.EnqueuedTime);
                AddProperty(WellKnownPropertyKeys.Subject, message.Subject);
                foreach (var kvp in message.Properties.NullSafeEnumerable().Where(z => !WellKnownPropertyKeys.All.Contains(z.Key)))
                {
                    AddProperty(kvp.Key, kvp.Value, decomposeValue: true);
                }
            }

            await processAsync(message);
        }
        finally
        {
            while (scopes.Count > 0)
            {
                Stuff.Dispose(scopes.Pop());
            }
        }
    }
}

