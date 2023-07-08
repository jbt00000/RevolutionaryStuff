using System.IO;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Serialization;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.JsonStore.Entities;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

internal class DefaultJsonEntityCosmosSerializer<TTenantFinder> : CosmosSerializer
    where TTenantFinder : ITenantFinder<string>
{
    private static readonly RevolutionaryStuff.Data.JsonStore.Serialization.Json.IJsonSerializer Serializer = JsonEntity.Serializer;

    private readonly TTenantFinder TenantFinder;

    public DefaultJsonEntityCosmosSerializer(TTenantFinder tenantFinder)
    {
        ArgumentNullException.ThrowIfNull(tenantFinder);

        TenantFinder = tenantFinder;
    }

    private string TenantIdField;
    private string TenantId
        => TenantIdField ??= TenantFinder.GetTenantIdAsync().ExecuteSynchronously();

    private static readonly System.Text.Encoding UTF8 = new System.Text.UTF8Encoding(false);

    /// <remarks>https://github.com/Azure/azure-cosmos-dotnet-v3/issues/1194</remarks>
    public override Stream ToStream<T>(T input)
    {
        if (input is ITenanted<string> it)
        {
            if (it.TenantId != TenantId && it.TenantId != default)
            {
                throw new CrossTenantException(it.TenantId, TenantId, it);
            }
            it.TenantId = TenantId;
        }
        var st = new MemoryStream();
        using (var sw = new StreamWriter(st, UTF8, 1024 * 1024, true))
        {
            var json = Serializer.ToJson(input);
            sw.Write(json);
            sw.Flush();
        }
        st.Position = 0;
        return st;
    }

    public override T FromStream<T>(Stream stream)
    {
        Requires.ReadableStreamArg(stream);

        var json = stream.ReadToEnd();
        stream.Close(); //in CosmosJsonSerializerWrapper, MS checks and wants the stream to be closed after the object is deserialized
        return Deserialize<T>(json);
    }

    public async Task<T> FromStreamAsync<T>(Stream stream)
    {
        Requires.ReadableStreamArg(stream);
        var json = await stream.ReadToEndAsync();
        return Deserialize<T>(json);
    }

    public static T Deserialize<T>(string json, ISerializationBinder binder = null)
        => Serializer.FromJson<T>(json);
}
