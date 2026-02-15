using System.IO;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Data.Cosmos;

public class JsonSerializer2CosmosSerializerAdaptor(IJsonSerializer JsonSerializer) : CosmosLinqSerializer
{
    public override Stream ToStream<T>(T input)
    {
        // Single-pass serialization directly to stream
        var memoryStream = new MemoryStream();
        JsonSerializer.Serialize(input, memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (stream.CanSeek && stream.Length == 0)
            {
                return default;
            }

            // Single-pass deserialization directly from stream
            return JsonSerializer.FromStream<T>(stream);
        }
    }

    public override string SerializeMemberName(MemberInfo memberInfo)
        => JsonSerializer.GetMemberName(memberInfo);
}
