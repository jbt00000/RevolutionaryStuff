using System.Threading;
using Dapr.Client;

namespace RevolutionaryStuff.Dapr.Services.PubSub;

public interface ITypedTopicPublisher<TData>
{
    /// <summary>
    /// Publishes an event to the specified topic.
    /// </summary>
    /// <param name="data">The data that will be JSON serialized and provided as the event payload.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <typeparam name="TData">The type of the data that will be JSON serialized and provided as the event payload.</typeparam>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task PublishEventAsync(
        TData data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an event to the specified topic.
    /// </summary>
    /// <param name="data">The data that will be JSON serialized and provided as the event payload.</param>
    /// <param name="metadata">
    /// A collection of metadata key-value pairs that will be provided to the pubsub. The valid metadata keys and values 
    /// are determined by the type of pubsub component used.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <typeparam name="TData">The type of the data that will be JSON serialized and provided as the event payload.</typeparam>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task PublishEventAsync(
        TData data,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// // Bulk Publishes multiple events to the specified topic.
    /// </summary>
    /// <param name="events">The list of events to be published.</param>
    /// <param name="metadata">The metadata to be set at the request level for the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task<BulkPublishResponse<TData>> BulkPublishEventAsync(
        IReadOnlyList<TData> events,
        Dictionary<string, string> metadata = default,
        CancellationToken cancellationToken = default);
}
