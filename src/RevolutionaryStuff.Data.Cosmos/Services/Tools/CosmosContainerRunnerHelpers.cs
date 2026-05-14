using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace RevolutionaryStuff.Data.Cosmos.Services.Tools;

public static class CosmosContainerRunnerHelpers
{
    /// <summary>Settings that control how <see cref="EachContainerRunAsync{TSettings,TResult}"/> behaves.</summary>
    public record RunSettings
    {
        /// <summary>
        /// When non-null, only containers whose Id is in this set are processed.
        /// When null or empty, all containers are processed.
        /// </summary>
        public IReadOnlyCollection<string>? ContainerIds { get; init; }
    }

    /// <summary>Settings that control how <see cref="EachContainerRunAsync{TSettings,TResult}"/> behaves, including tool-specific settings.</summary>
    public record RunSettings<TToolSettings> : RunSettings
    {
        /// <summary>Passed to each inner <see cref="ICosmosContainerRunner{TSettings,TResult}.RunAsync"/> call.</summary>
        public required TToolSettings ToolSettings { get; init; }
    }

    /// <summary>Result envelope returned for a single container.</summary>
    public record ContainerRunResult<TResult>(string ContainerId, TResult Result);

    /// <summary>
    /// Convenience wrapper for <see cref="ICosmosFieldCopier"/>. Resolves the copier from
    /// <paramref name="serviceProvider"/> once per container and runs it across every container
    /// in <paramref name="databaseId"/> that matches <paramref name="containerIds"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// await cosmosClient.EachContainerRunFieldCopierAsync(
    ///     serviceProvider, databaseId,
    ///     new CosmosFieldCopierConfig { SourceFieldName = "old", DestFieldName = "new" },
    ///     ct: ct);
    /// </code>
    /// </example>
    public static Task<IReadOnlyList<ContainerRunResult<CosmosFieldCopierResult>>> EachContainerRunFieldCopierAsync(
        this CosmosClient cosmosClient,
        IServiceProvider serviceProvider,
        string databaseId,
        CosmosFieldCopierConfig config,
        IReadOnlyCollection<string>? containerIds = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        return cosmosClient.EachContainerRunAsync<ICosmosFieldCopier, CosmosFieldCopierConfig, CosmosFieldCopierResult>(
            serviceProvider,
            databaseId,
            new RunSettings<CosmosFieldCopierConfig> { ToolSettings = config, ContainerIds = containerIds },
            ct);
    }

    /// <summary>
    /// Enumerates every container in <paramref name="databaseId"/>, resolves <typeparamref name="TRunner"/>
    /// from <paramref name="serviceProvider"/> once per container (transient-safe), and calls
    /// <see cref="ICosmosContainerRunner{TSettings,TResult}.RunAsync"/> for each.
    /// </summary>
    /// <remarks>
    /// All three type parameters must be specified explicitly because C# cannot infer
    /// <typeparamref name="TSettings"/> and <typeparamref name="TResult"/> from the constraint alone:
    /// <code>
    /// await cosmosClient.EachContainerRunAsync&lt;ICosmosFieldCopier, CosmosFieldCopierConfig, CosmosFieldCopierResult&gt;(
    ///     serviceProvider, databaseId,
    ///     new RunSettings&lt;CosmosFieldCopierConfig&gt; { ToolSettings = cfg },
    ///     ct);
    /// </code>
    /// </remarks>
    public static Task<IReadOnlyList<ContainerRunResult<TResult>>> EachContainerRunAsync<TRunner, TSettings, TResult>(
        this CosmosClient cosmosClient,
        IServiceProvider serviceProvider,
        string databaseId,
        RunSettings<TSettings> settings,
        CancellationToken ct = default)
        where TRunner : ICosmosContainerRunner<TSettings, TResult>
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        return cosmosClient.EachContainerRunAsync(
            () => serviceProvider.GetRequiredService<TRunner>(),
            databaseId, settings, ct);
    }

    /// <summary>
    /// Enumerates every container in <paramref name="databaseId"/>, calls
    /// <see cref="ICosmosContainerRunner{TSettings,TResult}.RunAsync"/> on <paramref name="runner"/> for each.
    /// A single shared instance is used — suitable when the runner is stateless or already scoped by the caller.
    /// </summary>
    /// <remarks>
    /// Type parameters are fully inferred from <paramref name="runner"/>:
    /// <code>
    /// await cosmosClient.EachContainerRunAsync(
    ///     sp.GetRequiredService&lt;ICosmosFieldCopier&gt;(),
    ///     databaseId,
    ///     new RunSettings&lt;CosmosFieldCopierConfig&gt; { ToolSettings = cfg },
    ///     ct);
    /// </code>
    /// </remarks>
    public static Task<IReadOnlyList<ContainerRunResult<TResult>>> EachContainerRunAsync<TSettings, TResult>(
        this CosmosClient cosmosClient,
        ICosmosContainerRunner<TSettings, TResult> runner,
        string databaseId,
        RunSettings<TSettings> settings,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(runner);
        return cosmosClient.EachContainerRunAsync(() => runner, databaseId, settings, ct);
    }

    /// <summary>
    /// Core implementation. Enumerates every container in <paramref name="databaseId"/>, invokes
    /// <paramref name="runnerFactory"/> once per container so each gets its own instance (transient-safe
    /// and future-parallel-safe), then calls
    /// <see cref="ICosmosContainerRunner{TSettings,TResult}.RunAsync"/>.
    /// </summary>
    public static async Task<IReadOnlyList<ContainerRunResult<TResult>>> EachContainerRunAsync<TSettings, TResult>(
        this CosmosClient cosmosClient,
        Func<ICosmosContainerRunner<TSettings, TResult>> runnerFactory,
        string databaseId,
        RunSettings<TSettings> settings,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(runnerFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseId);
        ArgumentNullException.ThrowIfNull(settings);

        var database = cosmosClient.GetDatabase(databaseId);
        var containerIds = await ListContainerIdsAsync(database, ct);

        var results = new List<ContainerRunResult<TResult>>();

        foreach (var containerId in containerIds)
        {
            if (settings.ContainerIds?.Count > 0 &&
                !settings.ContainerIds.Contains(containerId))
            {
                continue;
            }

            var container = database.GetContainer(containerId);
            var result = await runnerFactory().RunAsync(container, settings.ToolSettings, ct);
            results.Add(new ContainerRunResult<TResult>(containerId, result));
        }

        return results.AsReadOnly();
    }

    private static async Task<List<string>> ListContainerIdsAsync(Database database, CancellationToken ct)
    {
        var list = new List<string>();
        var iterator = database.GetContainerQueryIterator<ContainerProperties>();
        while (iterator.HasMoreResults)
            foreach (var props in await iterator.ReadNextAsync(ct))
                list.Add(props.Id);
        return list;
    }
}
