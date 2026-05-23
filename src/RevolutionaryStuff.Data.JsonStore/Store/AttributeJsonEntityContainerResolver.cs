using System.Collections.Concurrent;
using RevolutionaryStuff.Data.JsonStore.Entities;

namespace RevolutionaryStuff.Data.JsonStore.Store;

/// <summary>
/// Resolves container IDs from the <see cref="JsonEntityContainerIdAttribute"/> decoration.
/// Walks the entity's base-class chain (stopping before <c>object</c>).
/// Returns <c>null</c> if no attribute is found anywhere in the chain.
/// Results (including misses) are cached for the lifetime of this singleton.
/// </summary>
internal sealed class AttributeJsonEntityContainerResolver : IJsonEntityContainerResolver
{
    private readonly ConcurrentDictionary<Type, string?> Cache = new();

    public int Order => 2000;

    public string? ResolveContainerId(Type entityType)
        => Cache.GetOrAdd(entityType, Resolve);

    private static string? Resolve(Type entityType)
    {
        var t = entityType;
        while (t != null && t != typeof(object))
        {
            var attr = t.GetCustomAttribute<JsonEntityContainerIdAttribute>(inherit: false);
            if (attr != null)
                return attr.ContainerId;
            t = t.BaseType;
        }

        return null;
    }
}
