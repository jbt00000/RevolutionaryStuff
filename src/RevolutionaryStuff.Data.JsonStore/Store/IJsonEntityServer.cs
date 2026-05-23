using RevolutionaryStuff.Data.JsonStore.Entities;

namespace RevolutionaryStuff.Data.JsonStore.Store;

public interface IJsonEntityServer
{
    //TODO: change most of these references to taking a containerKey instead of a containerId, as the difference matters
    IJsonEntityContainer GetContainer(string containerId);

    /// <summary>
    /// Resolves the container ID for the given entity type.
    /// The default implementation falls back to <see cref="JsonEntityContainerIdAttribute.GetContainerId(Type)"/>,
    /// which is safe for non-DI scenarios such as unit tests.
    /// Implementations backed by DI (e.g. <c>CosmosJsonEntityServer</c>) override this to walk
    /// the registered <see cref="IJsonEntityContainerResolver"/> chain instead.
    /// </summary>
    string ResolveContainerId(Type entityType)
        => JsonEntityContainerIdAttribute.GetContainerId(entityType);

    IJsonEntityContainer GetContainer<TEntity>()
        where TEntity : JsonEntity
        => GetContainer(ResolveContainerId(typeof(TEntity)));
}
