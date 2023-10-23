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

#if false
    public static void AddObject(this IConfigurationBuilder builder, object o, string objectName = null, bool excludeNullMembers = false)
    {
        if (o == null) return;
        var filename = Path.GetTempFileName();
        string json;
        try
        {
            var ot = o.GetType();
            json = ot.GetCustomAttribute<DataContractAttribute>() != null
                ? o.GetType().GetJsonSerializer().WriteObjectToString(o)
                : JsonHelpers.ToJson(o);
        }
        catch (InvalidDataContractException)
        {
            json = JsonHelpers.ToJson(o);
        }
        if (excludeNullMembers)
        {
            json = RegexHelpers.Common.NullJsonMember.Replace(json, "");
        }
        if (objectName != null)
        {
            json = $"{{\"{objectName}\": {json} }}";
        }
        File.WriteAllText(filename, json);
        builder.AddJsonFile(filename);
    }

    private class PreconfiguredOptions<TOptions> : IOptions<TOptions> where TOptions : class, new()
    {

        public PreconfiguredOptions(TOptions val)
        {
            Value = val;
        }

        public TOptions Value { get; }
    }

    public static IOptions<TOptions> CreateOptions<TOptions>(TOptions val) where TOptions : class, new()
    {
        return new PreconfiguredOptions<TOptions>(val);
    }

    public static IConfiguration CreateConfigurationFromFilename(params string[] filenames)
    {
        var builder = new ConfigurationBuilder();
        if (filenames != null)
        {
            foreach (var filename in filenames)
            {
                var ext = Path.GetExtension(filename).ToLower();
                switch (ext)
                {
                    case ".json":
                        builder.AddJsonFile(filename, optional: false, reloadOnChange: false);
                        break;
                    default:
                        throw new NotSupportedException($"[{ext}] is not supported in API CreateConfigurationFromFilename");
                }
            }
        }
        return builder.Build();
    }
#endif

    public static T Get<T>(this IConfiguration configuration, string sectionName) where T : new()
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Requires.Text(sectionName);

        var ret = new T();
        configuration.Bind(sectionName, ret);
        return ret;
    }
}
