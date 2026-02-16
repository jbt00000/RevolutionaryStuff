using System.IO;
using System.Reflection;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.Data.JsonStore.Serialization.Json;

public interface IJsonSerializer
{
    static IJsonSerializer Default { get; set; } = RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.SystemTextJsonSerializer.Instance;

    object? FromJson(string json, Type t);
    string ToJson(object o);
    string GetMemberName(MemberInfo mi);

    #region Default Implementation

    void Serialize(object o, Stream st)
        => st.Write(ToJson(o));

    object FromStream(Stream st, Type t)
    {
        try
        {
            var json = st.ReadToEnd();
            return FromJson(json, t);
        }
        finally
        {
            st.Dispose();
        }
    }

    T FromJson<T>(string json)
        => (T)FromJson(json, typeof(T));

    T FromStream<T>(Stream st)
        => (T)FromStream(st, typeof(T));

    T Clone<T>(T obj)
    {
        if (obj == null) return obj;
        var json = ToJson(obj);
        return FromJson<T>(json);
    }

    #endregion
}
