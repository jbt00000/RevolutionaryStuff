namespace RevolutionaryStuff.Data.JsonStore.Store;

/// <summary>
/// Resolves a Cosmos container ID for a given entity type.
/// Multiple resolvers can be registered; they are evaluated in ascending <see cref="Order"/>,
/// and the first non-null result wins.
/// </summary>
/// <remarks>
/// Conventional order values:
/// <list type="table">
///   <item><term>&lt; 0</term><description>Emergency overrides registered by consumers</description></item>
///   <item><term>0 (default)</term><description>Normal custom resolvers (e.g. applet/solution resolvers)</description></item>
///   <item><term>1000</term><description><c>ConfigJsonEntityContainerResolver</c> — reads <c>IOptions&lt;JsonEntityContainerResolverConfig&gt;</c></description></item>
///   <item><term>2000</term><description><c>AttributeJsonEntityContainerResolver</c> — reads <c>[JsonEntityContainerId]</c></description></item>
/// </list>
/// </remarks>
public interface IJsonEntityContainerResolver
{
    /// <summary>
    /// Determines call order. Lower values have higher priority and are called first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Returns the container ID for <paramref name="entityType"/>,
    /// or <c>null</c> if this resolver cannot handle the type.
    /// </summary>
    string? ResolveContainerId(Type entityType);
}
