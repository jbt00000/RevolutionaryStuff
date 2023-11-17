using System.IO;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.Data.JsonStore.Serialization.Json;

public interface IJsonSerializer
{
    public static IJsonSerializer Default { get; set; } = RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.DefaultJsonSerializer.Instance;

    object? FromJson(string json, Type t);
    string ToJson(object o);

    #region Default Implementation

    public void Serialize(object o, Stream st)
        => st.Write(ToJson(o));

    T FromJson<T>(string json)
        => (T)FromJson(json, typeof(T));

    T Clone<T>(T obj)
    {
        if (obj == null) return obj;
        var json = ToJson(obj);
        return FromJson<T>(json);
    }

    #endregion
}
