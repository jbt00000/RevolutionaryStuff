namespace RevolutionaryStuff.Data.JsonStore.Entities;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class JsonEntityContainerIdAttribute : Attribute
{
    public readonly string ContainerId;

    public JsonEntityContainerIdAttribute(string containerId)
    {
        ContainerId = containerId;
    }

    public static string GetContainerId<TEntity>()
        => GetContainerId(typeof(TEntity));

    public static string GetContainerId(Type tEntity)
        => tEntity.GetCustomAttribute<JsonEntityContainerIdAttribute>()?.ContainerId
           ?? throw new InvalidOperationException(
               $"No [JsonEntityContainerId] attribute found on '{tEntity.FullName}' or its base classes.");
}
