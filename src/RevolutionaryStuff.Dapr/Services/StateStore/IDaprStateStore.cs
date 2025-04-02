using System.Threading;
using Dapr;
using Dapr.Client;

namespace RevolutionaryStuff.Dapr.Services.StateStore;

public interface IDaprStateStore
{
    string StoreName { get; }

    /// <summary>
    /// Gets the current value associated with the <paramref name="key" /> from the Dapr state store.
    /// </summary>
    /// <param name="key">The state key.</param>
    /// <param name="consistencyMode">The consistency mode <see cref="ConsistencyMode" />.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <typeparam name="TValue">The data type of the value to read.</typeparam>
    /// <returns>A <see cref="Task{T}" /> that will return the value when the operation has completed.</returns>
    Task<TValue> GetStateAsync<TValue>(string key, ConsistencyMode? consistencyMode = default, IReadOnlyDictionary<string, string?>? metadata = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of values associated with the <paramref name="keys" /> from the Dapr state store.
    /// </summary>
    /// <param name="keys">The list of keys to get values for.</param>
    /// <param name="parallelism">The number of concurrent get operations the Dapr runtime will issue to the state store. a value equal to or smaller than 0 means max parallelism.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{IReadOnlyList}" /> that will return the list of values when the operation has completed.</returns>
    Task<IReadOnlyList<BulkStateItem>> GetBulkStateAsync(IReadOnlyList<string> keys, int? parallelism, IReadOnlyDictionary<string, string?>? metadata = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of deserialized values associated with the <paramref name="keys" /> from the Dapr state store. This overload should be used
    /// if you expect the values of all the retrieved items to match the shape of the indicated <typeparam name="TValue"/>. If you expect that
    /// the values may differ in type from one another, do not specify the type parameter and instead use the original <see cref="GetBulkStateAsync"/> method
    /// so the serialized string values will be returned instead.
    /// </summary>
    /// <param name="keys">The list of keys to get values for.</param>
    /// <param name="parallelism">The number of concurrent get operations the Dapr runtime will issue to the state store. a value equal to or smaller than 0 means max parallelism.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{IReadOnlyList}" /> that will return the list of deserialized values when the operation has completed.</returns>
    Task<IReadOnlyList<BulkStateItem<TValue>>> GetBulkStateAsync<TValue>(IReadOnlyList<string> keys, int? parallelism, IReadOnlyDictionary<string, string?>? metadata = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a list of <paramref name="items" /> to the Dapr state store.
    /// </summary>
    /// <param name="items">The list of items to save.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task SaveBulkStateAsync<TValue>(IReadOnlyList<SaveStateItem<TValue>> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a list of <paramref name="items" /> from the Dapr state store.
    /// </summary>
    /// <param name="items">The list of items to delete</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task DeleteBulkStateAsync(IReadOnlyList<BulkDeleteStateItem> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current value associated with the <paramref name="key" /> from the Dapr state store and an ETag.
    /// </summary>
    /// <typeparam name="TValue">The data type of the value to read.</typeparam>
    /// <param name="key">The state key.</param>
    /// <param name="consistencyMode">The consistency mode <see cref="ConsistencyMode" />.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{T}" /> that will return the value when the operation has completed.  This wraps the read value and an ETag.</returns>
    Task<(TValue value, string etag)> GetStateAndETagAsync<TValue>(string key, ConsistencyMode? consistencyMode = default, IReadOnlyDictionary<string, string?>? metadata = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a <see cref="StateEntry{T}" /> for the current value associated with the <paramref name="key" /> from
    /// the Dapr state store.
    /// </summary>
    /// <param name="key">The state key.</param>
    /// <param name="consistencyMode">The consistency mode <see cref="ConsistencyMode" />.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <typeparam name="TValue">The type of the data that will be JSON deserialized from the state store response.</typeparam>
    /// <returns>A <see cref="Task" /> that will return the <see cref="StateEntry{T}" /> when the operation has completed.</returns>
    Task<StateEntry<TValue>> GetStateEntryAsync<TValue>(string key, ConsistencyMode? consistencyMode = default, IReadOnlyDictionary<string, string?>? metadata = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the provided <paramref name="value" /> associated with the provided <paramref name="key" /> to the Dapr state
    /// store.
    /// </summary>
    /// <param name="key">The state key.</param>
    /// <param name="value">The data that will be JSON serialized and stored in the state store.</param>        
    /// <param name="stateOptions">Options for performing save state operation.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <typeparam name="TValue">The type of the data that will be JSON serialized and stored in the state store.</typeparam>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task SaveStateAsync<TValue>(
        string key,
        TValue value,
        StateOptions? stateOptions = default,
        IReadOnlyDictionary<string, string?>? metadata = default,
        CancellationToken cancellationToken = default);

    async Task<TValue> AtomicStateActAsync<TValue>(string key, Func<TValue, Task<TValue>> actAsync, StateOptions? stateOptions = default, IReadOnlyDictionary<string, string?>? metadata = default, CancellationToken cancellationToken = default)
    {
        for (; !cancellationToken.IsCancellationRequested;)
        {
            var entry = await GetStateEntryAsync<TValue>(key, ConsistencyMode.Strong, metadata, cancellationToken);
            var newVal = await actAsync(entry.Value);
            if (await TrySaveStateAsync(key, newVal, entry.ETag, stateOptions, metadata, cancellationToken))
                return newVal;
        }
        throw new TaskCanceledException();
    }

    /// <summary>
    /// Tries to save the state <paramref name="value" /> associated with the provided <paramref name="key" /> using the 
    /// <paramref name="etag"/> to the Dapr state. State store implementation will allow the update only if the attached ETag matches with the latest ETag in the state store.
    /// store.
    /// </summary>
    /// <param name="key">The state key.</param>
    /// <param name="value">The data that will be JSON serialized and stored in the state store.</param>
    /// <param name="etag">An ETag.</param>
    /// <param name="stateOptions">Options for performing save state operation.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <typeparam name="TValue">The type of the data that will be JSON serialized and stored in the state store.</typeparam>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.  If the wrapped value is true the operation succeeded.</returns>
    Task<bool> TrySaveStateAsync<TValue>(
        string key,
        TValue value,
        string etag,
        StateOptions? stateOptions = default,
        IReadOnlyDictionary<string, string?>? metadata = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the provided <paramref name="operations" /> to the Dapr state
    /// store.
    /// </summary>
    /// <param name="operations">A list of StateTransactionRequests.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task ExecuteStateTransactionAsync(
        IReadOnlyList<StateTransactionRequest> operations,
        IReadOnlyDictionary<string, string?>? metadata = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the value associated with the provided <paramref name="key" /> in the Dapr state store.
    /// </summary>
    /// <param name="key">The state key.</param>
    /// <param name="stateOptions">A <see cref="StateOptions" />.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task DeleteStateAsync(
        string key,
        StateOptions? stateOptions = default,
        IReadOnlyDictionary<string, string?>? metadata = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to delete the the state associated with the provided <paramref name="key" /> using the 
    /// <paramref name="etag"/> from the Dapr state. State store implementation will allow the delete only if the attached ETag matches with the latest ETag in the state store.
    /// </summary>
    /// <param name="key">The state key.</param>
    /// <param name="etag">An ETag.</param>
    /// <param name="stateOptions">A <see cref="StateOptions" />.</param>
    /// <param name="metadata">A collection of metadata key-value pairs that will be provided to the state store. The valid metadata keys and values are determined by the type of state store used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.  If the wrapped value is true the operation suceeded.</returns>
    Task<bool> TryDeleteStateAsync(
        string key,
        string etag,
        StateOptions? stateOptions = default,
        IReadOnlyDictionary<string, string?>? metadata = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the specified statestore with the given query. The query is a JSON representation of the query as described by the Dapr QueryState API.
    /// Note that the underlying statestore must support queries.
    /// </summary>
    /// <param name="jsonQuery">A JSON formatted query string.</param>
    /// <param name="metadata">Metadata to send to the statestore.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <typeparam name="TValue">The data type of the value to read.</typeparam>
    /// <returns>A <see cref="StateQueryResponse{TValue}"/> that may be paginated, use <see cref="StateQueryResponse{TValue}.Token"/> to continue the query.</returns>
    Task<StateQueryResponse<TValue>> QueryStateAsync<TValue>(
        string jsonQuery,
        IReadOnlyDictionary<string, string?>? metadata = default,
        CancellationToken cancellationToken = default);
}
