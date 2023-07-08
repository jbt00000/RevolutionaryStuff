namespace RevolutionaryStuff.Data.JsonStore.Serialization.Json;

public interface IJsonSerializer
{
    T? FromJson<T>(string json);
    string ToJson(object o);
}
