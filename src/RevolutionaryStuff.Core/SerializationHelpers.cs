using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core;

public static class SerializationHelpers
{
    extension(Type source)
    {
        public DataContractJsonSerializer GetJsonSerializer()
                => PermaCache.FindOrCreate(
                    source,
                    () => new DataContractJsonSerializer(source));
    }

    public static DataContractJsonSerializer GetJsonSerializer<TSerializationType>()
        => GetJsonSerializer(typeof(TSerializationType));

    extension(DataContractJsonSerializer source)
    {
        public T ReadObjectFromString<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            using var st = StreamHelpers.Create(json);
            return (T)source.ReadObject(st);
        }
        public string WriteObjectToString(object o)
        {
            using var st = new MemoryStream();
            source.WriteObject(st, o);
            st.Position = 0;
            return st.ReadToEnd();
        }
    }



    extension(DataContractSerializer source)
    {
        public T ReadObjectFromString<T>(string xml) where T : class
        {
            if (string.IsNullOrEmpty(xml)) return null;
            try
            {
                using var st = StreamHelpers.Create(xml, System.Text.Encoding.UTF8);
                return (T)source.ReadObject(st);
            }
            catch (System.Xml.XmlException)
            {
                using var st = StreamHelpers.Create(xml, System.Text.Encoding.Unicode);
                return (T)source.ReadObject(st);
            }
        }
        public string WriteObjectToString(object o)
        {
            using var st = new MemoryStream();
            source.WriteObject(st, o);
            st.Position = 0;
            return st.ReadToEnd();
        }
    }


}
