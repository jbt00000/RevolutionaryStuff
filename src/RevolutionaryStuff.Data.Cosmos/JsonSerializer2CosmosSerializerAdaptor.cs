using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Azure.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Data.Cosmos;
public class JsonSerializer2CosmosSerializerAdaptor : CosmosLinqSerializer
{
    private readonly IJsonSerializer JsonSerializer;

    public JsonSerializer2CosmosSerializerAdaptor(IJsonSerializer jsonSerializer)
    {
        ArgumentNullException.ThrowIfNull(jsonSerializer);
        JsonSerializer = jsonSerializer;
    }

    public override Stream ToStream<T>(T input)
    {
        var json = JsonSerializer.ToJson(input);
        var bytes = Encoding.UTF8.GetBytes(json);
        return new MemoryStream(bytes, false);
    }

    public override T FromStream<T>(Stream stream)
    {
        try
        {
            var json = stream.ReadToEnd();
            return JsonSerializer.FromJson<T>(json);
        }
        finally
        {
            //rules of parent class demand disposing of stream in all cases
            stream.Dispose();
        }
    }

    public override string SerializeMemberName(MemberInfo memberInfo)
        => JsonSerializer.GetMemberName(memberInfo);
}
