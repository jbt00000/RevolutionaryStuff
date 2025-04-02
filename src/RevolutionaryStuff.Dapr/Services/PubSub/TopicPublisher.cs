using System.Net.Mime;
using System.Threading;
using Dapr.Client;

namespace RevolutionaryStuff.Dapr.Services.PubSub;

public class TopicPublisher(DaprClient _dapr, string _pubsubName, string _topicName) : ITopicPublisher
{
    Task<BulkPublishResponse<TValue>> ITopicPublisher.BulkPublishEventAsync<TValue>(IReadOnlyList<TValue> events, Dictionary<string, string> metadata, CancellationToken cancellationToken)
        => _dapr.BulkPublishEventAsync(_pubsubName, _topicName, events, metadata, cancellationToken);

    Task ITopicPublisher.PublishByteEventAsync(ReadOnlyMemory<byte> data, string? dataContentType, Dictionary<string, string>? metadata, CancellationToken cancellationToken)
        => _dapr.PublishByteEventAsync(_pubsubName, _topicName, data, dataContentType ?? MediaTypeNames.Application.Json, metadata, cancellationToken);

    Task ITopicPublisher.PublishEventAsync<TData>(TData data, CancellationToken cancellationToken)
        => _dapr.PublishEventAsync(_pubsubName, _topicName, data, cancellationToken);

    Task ITopicPublisher.PublishEventAsync<TData>(TData data, Dictionary<string, string> metadata, CancellationToken cancellationToken)
        => _dapr.PublishEventAsync(_pubsubName, _topicName, data, metadata, cancellationToken);

    Task ITopicPublisher.PublishEventAsync(CancellationToken cancellationToken)
        => _dapr.PublishEventAsync(_pubsubName, _topicName, cancellationToken);

    Task ITopicPublisher.PublishEventAsync(Dictionary<string, string> metadata, CancellationToken cancellationToken)
        => _dapr.PublishEventAsync(_pubsubName, _topicName, metadata, cancellationToken);
}
