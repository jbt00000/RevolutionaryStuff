using System.IO;
using Microsoft.Extensions.Configuration;

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

    public static T Get<T>(this IConfiguration configuration, string sectionName) where T : new()
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Requires.Text(sectionName);

        var ret = new T();
        configuration.Bind(sectionName, ret);
        return ret;
    }
}
