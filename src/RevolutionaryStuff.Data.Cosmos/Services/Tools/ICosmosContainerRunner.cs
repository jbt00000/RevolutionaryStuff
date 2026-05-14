using System.Threading;
using Microsoft.Azure.Cosmos;

namespace RevolutionaryStuff.Data.Cosmos.Services.Tools;

/// <summary>
/// Abstraction for a single-container operation that can be fanned out across multiple
/// containers via <see cref="CosmosContainerRunnerHelpers.EachContainerRunAsync{TRunner,TSettings,TResult}"/>.
/// </summary>
public interface ICosmosContainerRunner<TSettings, TResult>
{
    /// <summary>Executes the runner against the specified container using the provided settings.</summary>
    Task<TResult> RunAsync(Container container, TSettings settings, CancellationToken ct = default);
}
