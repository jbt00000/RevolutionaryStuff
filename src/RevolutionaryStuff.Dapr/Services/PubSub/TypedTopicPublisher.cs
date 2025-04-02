using System.Threading;
using Dapr.Client;

namespace RevolutionaryStuff.Dapr.Services.PubSub;

public class TypedTopicPublisher<TData>(DaprClient _dapr, string _pubsubName, string _topicName) : ITypedTopicPublisher<TData>
{
    Task<BulkPublishResponse<TData>> ITypedTopicPublisher<TData>.BulkPublishEventAsync(IReadOnlyList<TData> events, Dictionary<string, string> metadata, CancellationToken cancellationToken)
        => _dapr.BulkPublishEventAsync(_pubsubName, _topicName, events, metadata, cancellationToken);

    Task ITypedTopicPublisher<TData>.PublishEventAsync(TData data, CancellationToken cancellationToken)
        => _dapr.PublishEventAsync(_pubsubName, _topicName, data, cancellationToken);

    Task ITypedTopicPublisher<TData>.PublishEventAsync(TData data, Dictionary<string, string> metadata, CancellationToken cancellationToken)
        => _dapr.PublishEventAsync(_pubsubName, _topicName, data, metadata, cancellationToken);
}
