using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

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
        var filename = Path.GetTempFileName();
        File.WriteAllText(filename, json);
        builder.AddJsonFile(filename);
    }

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

    public static O Get<O>(this IConfiguration configuration, string sectionName = null, bool throwOnExtraneousSettings = true) where O : new()
    {
        sectionName ??= typeof(O).Name;
        var section = configuration.GetSection(sectionName);
        return section.Get<O>(throwOnExtraneousSettings);
    }

    public static O Get<O>(this IConfigurationSection section, bool throwOnExtraneousSettings = true) where O : new()
    {
        return (O)section.Get(typeof(O), throwOnExtraneousSettings);
    }

    private static object Get(this IConfigurationSection section, Type t, bool throwOnExtraneousSettings = true)
    {
        object o = null;
        var ti = t.GetTypeInfo();
        var ci = ti.GetConstructor(Empty.TypeArray);
        if (ci != null)
        {
            o = ci.Invoke(Empty.ObjectArray);
            if (section != null)
            {
                var childSectionsByName = section.GetChildren().ToDictionary(z => z.Key, Comparers.CaseInsensitiveStringComparer);
                var propByName = o.GetType().GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance).ToDictionary(pi => pi.Name, Comparers.CaseInsensitiveStringComparer);
                foreach (var pi in propByName.Values)
                {
                    if (!childSectionsByName.TryGetValue(pi.Name, out var childSection)) continue;
                    object val;
                    var s = section[pi.Name];
                    val = s == null
                        ? childSection.GetChildren().HasData() ? childSection.Get(pi.PropertyType, throwOnExtraneousSettings) : null
                        : TypeHelpers.ConvertValue(pi.PropertyType, s);
                    pi.SetValue(o, val);
                }
                if (throwOnExtraneousSettings)
                {
                    var extras = childSectionsByName.Keys.Where(sn => !propByName.ContainsKey(sn)).ConvertAll(sn => new ArgumentOutOfRangeException(sn));
                    if (extras.Count > 0)
                    {
                        throw new AggregateException("Extraneous settings exist", extras);
                    }
                }
            }
        }
        else if (ti.IsArray)
        {
            var childSections = section.GetChildren().ToList();
            ci = ti.GetConstructor(new[] { typeof(int) });
            var arr = (Array)ci.Invoke(new object[] { childSections.Count });
            var elType = ti.GetElementType();
            for (var z = 0; z < childSections.Count; ++z)
            {
                var childSection = childSections[z];
                var val = childSection.GetChildren().HasData() ? childSection.Get(elType, false) : TypeHelpers.ConvertValue(elType, childSection.Value);
                arr.SetValue(val, z);
            }
            o = arr;
        }
        else
        {
            throw new NotSupportedException();
        }

        return o;
    }
}
