namespace RevolutionaryStuff.Data.JsonStore.Serialization.Json;

public interface IJsonSerializer
{
    object? FromJson(string json, Type t);
    string ToJson(object o);

    #region Default Implementation

    T? FromJson<T>(string json)
        => (T?)FromJson(json, typeof(T));

    #endregion
}
