using System.Threading;
using Dapr;
using Dapr.Client;

namespace RevolutionaryStuff.Dapr.Services.StateStore;

internal class DaprStateStore : IDaprStateStore
{
    private readonly DaprClient Dapr;
    private readonly string StoreName;

    public DaprStateStore(DaprClient dapr, string? storeName)
    {
        Requires.Text(storeName);
        Dapr = dapr;
        StoreName = storeName!;
    }

    string IDaprStateStore.StoreName => StoreName;

    Task IDaprStateStore.DeleteBulkStateAsync(IReadOnlyList<BulkDeleteStateItem> items, CancellationToken cancellationToken)
        => Dapr.DeleteBulkStateAsync(StoreName, items, cancellationToken);

    Task IDaprStateStore.DeleteStateAsync(string key, StateOptions? stateOptions, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.DeleteStateAsync(StoreName, key, stateOptions, metadata, cancellationToken);

    Task IDaprStateStore.ExecuteStateTransactionAsync(IReadOnlyList<StateTransactionRequest> operations, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.ExecuteStateTransactionAsync(StoreName, operations, metadata, cancellationToken);

    Task<IReadOnlyList<BulkStateItem>> IDaprStateStore.GetBulkStateAsync(IReadOnlyList<string> keys, int? parallelism, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.GetBulkStateAsync(StoreName, keys, parallelism, metadata, cancellationToken);

    Task<IReadOnlyList<BulkStateItem<TValue>>> IDaprStateStore.GetBulkStateAsync<TValue>(IReadOnlyList<string> keys, int? parallelism, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.GetBulkStateAsync<TValue>(StoreName, keys, parallelism, metadata, cancellationToken);

    Task<(TValue value, string etag)> IDaprStateStore.GetStateAndETagAsync<TValue>(string key, ConsistencyMode? consistencyMode, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.GetStateAndETagAsync<TValue>(StoreName, key, consistencyMode, metadata, cancellationToken);

    Task<TValue> IDaprStateStore.GetStateAsync<TValue>(string key, ConsistencyMode? consistencyMode, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.GetStateAsync<TValue>(StoreName, key, consistencyMode, metadata, cancellationToken);

    Task<StateEntry<TValue>> IDaprStateStore.GetStateEntryAsync<TValue>(string key, ConsistencyMode? consistencyMode, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.GetStateEntryAsync<TValue>(StoreName, key, consistencyMode, metadata, cancellationToken);

    Task<StateQueryResponse<TValue>> IDaprStateStore.QueryStateAsync<TValue>(string jsonQuery, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.QueryStateAsync<TValue>(StoreName, jsonQuery, metadata, cancellationToken);

    Task IDaprStateStore.SaveBulkStateAsync<TValue>(IReadOnlyList<SaveStateItem<TValue>> items, CancellationToken cancellationToken)
        => Dapr.SaveBulkStateAsync<TValue>(StoreName, items, cancellationToken);

    Task IDaprStateStore.SaveStateAsync<TValue>(string key, TValue value, StateOptions? stateOptions, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.SaveStateAsync<TValue>(StoreName, key, value, stateOptions, metadata, cancellationToken);

    Task<bool> IDaprStateStore.TryDeleteStateAsync(string key, string etag, StateOptions? stateOptions, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.TryDeleteStateAsync(StoreName, key, etag, stateOptions, metadata, cancellationToken);

    Task<bool> IDaprStateStore.TrySaveStateAsync<TValue>(string key, TValue value, string etag, StateOptions? stateOptions, IReadOnlyDictionary<string, string?>? metadata, CancellationToken cancellationToken)
        => Dapr.TrySaveStateAsync<TValue>(StoreName, key, value, etag, stateOptions, metadata, cancellationToken);
}
