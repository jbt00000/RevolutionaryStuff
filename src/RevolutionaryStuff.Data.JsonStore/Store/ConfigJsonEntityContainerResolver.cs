using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Data.JsonStore.Store;

/// <summary>
/// Resolves container IDs from <see cref="JsonEntityContainerResolverConfig"/>.
/// Walks the entity's base-class chain (stopping before <c>object</c>), checking both
/// the simple class name and the fully-qualified name at each step.
/// Results (including misses) are cached for the lifetime of this singleton.
/// </summary>
internal sealed class ConfigJsonEntityContainerResolver : IJsonEntityContainerResolver
{
    private readonly IOptions<JsonEntityContainerResolverConfig> ConfigOptions;
    private readonly ConcurrentDictionary<Type, string?> Cache = new();

    public ConfigJsonEntityContainerResolver(IOptions<JsonEntityContainerResolverConfig> configOptions)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ConfigOptions = configOptions;
    }

    public int Order => 1000;

    public string? ResolveContainerId(Type entityType)
        => Cache.GetOrAdd(entityType, Resolve);

    private string? Resolve(Type entityType)
    {
        var map = ConfigOptions.Value.ContainerIdByTypeName;
        if (map == null || map.Count == 0)
            return null;

        var t = entityType;
        while (t != null && t != typeof(object))
        {
            if (t.Name != null && map.TryGetValue(t.Name, out var byShortName))
                return byShortName;
            if (t.FullName != null && map.TryGetValue(t.FullName, out var byFullName))
                return byFullName;
            t = t.BaseType;
        }

        return null;
    }
}
