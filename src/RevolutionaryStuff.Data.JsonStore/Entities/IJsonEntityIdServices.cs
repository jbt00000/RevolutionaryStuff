namespace RevolutionaryStuff.Data.JsonStore.Entities;

public interface IJsonEntityIdServices
{
    string CreateId(Type entityDataType, string? name = null);
    void ThrowIfInvalid(Type entityDataType, string id);
}
