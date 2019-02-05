using RevolutionaryStuff.Core.Caching;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace RevolutionaryStuff.Core
{
    public static class SerializationHelpers
    {
        public static DataContractJsonSerializer GetJsonSerializer(this Type t)
            => Cache.DataCacher.FindOrCreateValue(
                Cache.CreateKey(typeof(SerializationHelpers), nameof(GetJsonSerializer), t), 
                () => new DataContractJsonSerializer(t));

        public static DataContractJsonSerializer GetJsonSerializer<TSerializationType>()
            => GetJsonSerializer(typeof(TSerializationType));

        public static T ReadObjectFromString<T>(this DataContractJsonSerializer ser, string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            using (var st = StreamHelpers.Create(json))
            {
                return (T)ser.ReadObject(st);
            }
        }

        public static string WriteObjectToString(this DataContractJsonSerializer ser, object o)
        {
            using (var st = new MemoryStream())
            {
                ser.WriteObject(st, o);
                st.Position = 0;
                return st.ReadToEnd();
            }
        }

        public static T ReadObjectFromString<T>(this DataContractSerializer ser, string xml) where T : class
        {
            if (string.IsNullOrEmpty(xml)) return null;
            try
            {
                using (var st = StreamHelpers.Create(xml, System.Text.Encoding.UTF8))
                {
                    return (T)ser.ReadObject(st);
                }
            }
            catch (System.Xml.XmlException)
            {
                using (var st = StreamHelpers.Create(xml, System.Text.Encoding.Unicode))
                {
                    return (T)ser.ReadObject(st);
                }
            }
        }

        public static string WriteObjectToString(this DataContractSerializer ser, object o)
        {
            using (var st = new MemoryStream())
            {
                ser.WriteObject(st, o);
                st.Position = 0;
                return st.ReadToEnd();
            }
        }
    }
}
