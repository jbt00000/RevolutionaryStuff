namespace RevolutionaryStuff.Data.JsonStore.Store;

/// <summary>
/// Configuration for <c>ConfigJsonEntityContainerResolver</c>.
/// Bind to the <see cref="ConfigSectionName"/> section in <c>appsettings.json</c>.
/// </summary>
/// <example>
/// <code language="json">
/// "JsonEntityContainerResolver": {
///   "ContainerIdByTypeName": {
///     "BaseCommunicationEntity": "communication",
///     "BaseCrmEntity": "crm"
///   }
/// }
/// </code>
/// </example>
public class JsonEntityContainerResolverConfig
{
    public const string ConfigSectionName = "JsonEntityContainerResolver";

    /// <summary>
    /// Maps a type name to a container ID.
    /// Keys may be the simple class name (e.g. <c>"BaseCrmEntity"</c>) or the fully-qualified name.
    /// The resolver checks every type in the entity's base-class chain, so a base-class entry covers all derived types.
    /// </summary>
    public Dictionary<string, string> ContainerIdByTypeName { get; set; } = [];
}
