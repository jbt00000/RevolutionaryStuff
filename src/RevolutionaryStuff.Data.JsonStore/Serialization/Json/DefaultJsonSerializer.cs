using System.IO;
using System.Text;
using Newtonsoft.Json;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.JsonStore.Serialization.Json;

internal class DefaultJsonSerializer : IJsonSerializer
{
    public static readonly IJsonSerializer Instance = new DefaultJsonSerializer();

    private readonly JsonSerializerSettings MyJsonSerializationSettings;

    private readonly JsonSerializer Serializer;

    private static readonly Encoding UTF8 = new UTF8Encoding(false);

    private void SerializeToJson(object o, Stream st)
    {
        Requires.WriteableStreamArg(st);
        using var sw = new StreamWriter(st, UTF8, 1024 * 1024, true);
        Serializer.Serialize(new BlankTypeRemovingJsonWriter(sw), o);
        sw.Flush();
    }

    private DefaultJsonSerializer()
    {
        var converters = new List<JsonConverter>(JsonConverterTypeAttribute.JsonConverters).FluentAdd(EnumMemberJsonConverter.Instance);

        Serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented,
            //            ContractResolver = TraffkContractResolver.Instance,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate,
            TypeNameHandling = TypeNameHandling.Objects,
            //            SerializationBinder = binder
        };
        converters.ForEach(Serializer.Converters.Add);

        MyJsonSerializationSettings = new()
        {
            Converters = converters
        };
    }

    string IJsonSerializer.ToJson(object o)
    {
        using var st = new MemoryStream();
        SerializeToJson(o, st);
        st.Position = 0;
        return Encoding.Default.GetString(st.ToArray());
    }

    T? IJsonSerializer.FromJson<T>(string json) 
        where T : default 
        => JsonConvert.DeserializeObject<T>(json, MyJsonSerializationSettings);
}
