using Dapr.Client;

namespace RevolutionaryStuff.Dapr.Services.StateStore;

internal class MyDaprStateStore(DaprClient _dapr, string? _storeName) : DaprStateStore(_dapr, _storeName), IMyDaprStateStore
{ }
