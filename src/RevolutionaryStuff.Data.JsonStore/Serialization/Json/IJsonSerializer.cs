namespace RevolutionaryStuff.Data.JsonStore.Serialization.Json;

public interface IJsonSerializer
{
    T? FromJson<T>(string json) where T : class;
    string ToJson(object o);
}
