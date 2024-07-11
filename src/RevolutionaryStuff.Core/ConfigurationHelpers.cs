using System.IO;
using System.Runtime.Serialization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core;

public static class ConfigurationHelpers
{
    public static string CreateKeyFromSegments(params string[] segments)
        => segments.Format(":");

    public static string GetString(this IConfiguration config, params string[] segments)
        => config[CreateKeyFromSegments(segments)];

    public static bool GetBool(this IConfiguration config, params string[] segments)
        => Parse.ParseBool(config.GetString(segments));

    public static void AddJsonString(this IConfigurationBuilder builder, string json)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Requires.Text(json);

        var buf = Raw.ToUTF8(json);
        var st = new MemoryStream(buf, false);
        builder.AddJsonStream(st);
    }

    public static void AddObject(this IConfigurationBuilder builder, object o, string objectName = null, bool excludeNullMembers = false)
    {
        if (o != null)
        {
            string json = JsonHelpers.ToJson(o);

            if (excludeNullMembers)
            {
                json = RegexHelpers.Common.NullJsonMember().Replace(json, "");
            }

            if (objectName != null)
            {
                json = $"{{\"{objectName}\": {json} }}";
            }

            builder.AddJsonString(json);
        }
    }

    public static T Get<T>(this IConfiguration configuration, string sectionName) where T : new()
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Requires.Text(sectionName);

        var ret = new T();
        configuration.Bind(sectionName, ret);
        (ret as IPostConfigure)?.PostConfigure();
        return ret;
    }
}
