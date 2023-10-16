using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RevolutionaryStuff.Core.Services.JsonSerializers.Newtonsoft.Converters;
public class EnumMemberJsonConverter : StringEnumConverter
{
    public static readonly EnumMemberJsonConverter Instance = new();

    private static readonly Dictionary<Type, Dictionary<string, object>> EnumByKeyByType = new();

    private EnumMemberJsonConverter()
    { }

    private static object ParseNullableEnumObjectWithEnumMemberValues(
      Type t,
      string val,
      bool caseSensitive = false)
    {
        if (string.IsNullOrEmpty(val))
        {
            return null;
        }

        Dictionary<string, object> orCreateValue;

        lock (EnumByKeyByType)
        {
            orCreateValue = EnumByKeyByType.FindOrCreate(t, () =>
            {
                var dictionary = caseSensitive ? new() : new Dictionary<string, object>(Comparers.CaseInsensitiveStringComparer);
                var enumType = t.IsNullableEnum() ? t.GenericTypeArguments[0] : t;
                foreach (var obj in Enum.GetValues(enumType))
                {
                    var member = enumType.GetMember(obj.ToString());
                    var element = ((IEnumerable<MemberInfo>)member)?.FirstOrDefault<MemberInfo>();
                    var customAttribute = element.GetCustomAttribute<EnumMemberAttribute>();
                    if (customAttribute != null)
                    {
                        dictionary[customAttribute.Value] = obj;
                    }
                    else
                    {
                        dictionary[element.Name] = obj;
                    }
                }
                return dictionary;
            });
        }

        return !string.IsNullOrEmpty(val) && orCreateValue.ContainsKey(val) ? (Enum)orCreateValue[val] : (object)null;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var enumString = reader.Value.ToString();
            var e = ParseNullableEnumObjectWithEnumMemberValues(objectType, enumString);
            return e;
        }
        return base.ReadJson(reader, objectType, existingValue, serializer);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var e = (Enum)value;
        var sval = e.EnumWithEnumMemberValuesToString();
        if (int.TryParse(sval, out _))
        {
            base.WriteJson(writer, value, serializer);
        }
        else
        {
            writer.WriteValue(sval);
        }
    }
}
