using RevolutionaryStuff.Data.JsonStore.Entities;

namespace RevolutionaryStuff.Data.JsonStore.Store;

public interface IJsonEntityServer
{
    IJsonEntityContainer GetContainer(string containerId);

    IJsonEntityContainer GetContainer<TEntity>()
        where TEntity : JsonEntity
        => GetContainer(JsonEntityContainerIdAttribute.GetContainerId<TEntity>());
}
