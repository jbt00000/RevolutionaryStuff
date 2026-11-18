using System.IO;
using Microsoft.Extensions.Configuration;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core;

public static class ConfigurationHelpers
{
    public static string CreateKeyFromSegments(params string[] segments)
        => segments.Format(":");

    extension(IConfiguration source)
    {
        public string GetString(params string[] segments)
                => source[CreateKeyFromSegments(segments)];
        public bool GetBool(params string[] segments)
                => Parse.ParseBool(source.GetString(segments));
        public T Get<T>(string sectionName) where T : new()
        {
            ArgumentNullException.ThrowIfNull(source);
            Requires.Text(sectionName);

            var ret = new T();
            source.Bind(sectionName, ret);
            (ret as IPostConfigure)?.PostConfigure();
            (ret as IValidate)?.Validate();
            return ret;
        }
    }



    extension(IConfigurationBuilder source)
    {
        public void AddJsonString(string json)
        {
            ArgumentNullException.ThrowIfNull(source);
            Requires.Text(json);

            var buf = Raw.ToUTF8(json);
            var st = new MemoryStream(buf, false);
            source.AddJsonStream(st);
        }
        public void AddObject(object o, string objectName = null, bool excludeNullMembers = false)
        {
            if (o != null)
            {
                var json = JsonHelpers.ToJson(o);

                if (excludeNullMembers)
                {
                    json = RegexHelpers.Common.NullJsonMember().Replace(json, "");
                }

                if (objectName != null)
                {
                    json = $"{{\"{objectName}\": {json} }}";
                }

                source.AddJsonString(json);
            }
        }
    }




}
