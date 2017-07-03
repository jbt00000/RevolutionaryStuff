using RevolutionaryStuff.Core.Caching;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace RevolutionaryStuff.Core
{
    public static class SerializationHelpers
    {
        private static readonly ICache<Type, DataContractJsonSerializer> JsonSerializerCache = Cache.CreateSynchronized<Type, DataContractJsonSerializer>();

        public static DataContractJsonSerializer GetJsonSerializer(this Type t)
        {
            return JsonSerializerCache.Do(t, () => new DataContractJsonSerializer(t));
        }
        public static DataContractJsonSerializer GetJsonSerializer<TSerializationType>()
        {
            return GetJsonSerializer(typeof(TSerializationType));
        }

        public static T ReadObjectFromString<T>(this DataContractJsonSerializer ser, string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            using (var st = new MemoryStream(Raw.String2Buf(json)))
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
