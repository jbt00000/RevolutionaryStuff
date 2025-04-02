using Dapr.Client;

namespace RevolutionaryStuff.Dapr.Services.StateStore;

internal class SharedDaprStateStore(DaprClient _dapr, string? _storeName) : DaprStateStore(_dapr, _storeName), ISharedDaprStateStore
{ }
