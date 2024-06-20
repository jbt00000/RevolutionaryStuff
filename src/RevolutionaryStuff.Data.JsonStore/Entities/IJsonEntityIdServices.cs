namespace RevolutionaryStuff.Data.JsonStore.Entities;

public interface IJsonEntityIdServices
{
    string CreateId(Type entityDataType, string? name = null, bool includeRandomCode = true);
    void ThrowIfInvalid(Type entityDataType, string id);
    bool IsValid(Type entityDataType, string id);

    #region Default Implementation

    void ThrowIfNotNullAndInvalid(Type entityDataType, string id)
    {
        if (id != null)
        {
            ThrowIfInvalid(entityDataType, id);
        }
    }

    void ThrowIfInvalid<T>(string id)
        where T : JsonEntity
        => ThrowIfInvalid(typeof(T), id);

    void ThrowIfNotNullAndInvalid<T>(string? id)
        where T : JsonEntity
    {
        if (id != null)
        {
            ThrowIfInvalid(typeof(T), id);
        }
    }

    bool IsValid<T>(string? id)
        where T : JsonEntity
        => IsValid(typeof(T), id);

    #endregion
}
