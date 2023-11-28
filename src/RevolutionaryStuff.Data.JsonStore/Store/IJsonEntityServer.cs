using RevolutionaryStuff.Data.JsonStore.Entities;

namespace RevolutionaryStuff.Data.JsonStore.Store;

public interface IJsonEntityServer
{
    //TODO: change most of these references to taking a containerKey instead of a containerId, as the difference matters
    IJsonEntityContainer GetContainer(string containerId);

    IJsonEntityContainer GetContainer<TEntity>()
        where TEntity : JsonEntity
        => GetContainer(JsonEntityContainerIdAttribute.GetContainerId<TEntity>());
}
