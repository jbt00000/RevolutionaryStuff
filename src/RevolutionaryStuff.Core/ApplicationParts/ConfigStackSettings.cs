using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Core.ApplicationParts;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ConfigStackSettingsAppSettingsAttribute : Attribute
{
    public readonly string BaseConfigFileName;

    public ConfigStackSettingsAppSettingsAttribute(string baseConfigFileName = null)
    {
        BaseConfigFileName = baseConfigFileName;
    }
}

public class ConfigStackSettings
{
    private const string ConfigSettingsJsonFileName = "configsettings.json";
    public const string ConfigSectionName = "ConfigStackSettings";

    public class ResourceInfo
    {
        public string AssemblyName { get; set; }
        public string BaseConfigFileName { get; set; }
        public override string ToString()
            => $"assembly=>{AssemblyName} baseConfig=>{BaseConfigFileName}";
    }

    public static ConfigStackSettings Discover(ConfigStackSettingsConfig config, Assembly a, ILogger logger)
    {
        List<(HashSet<string> PredecessorNames, ResourceInfo RI)> nodes = [];
        var ignoreExpr = new Regex(config.AutoDiscoveryIgnoreAssemblyNamePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        HashSet<string> testedAssemblyNames = [];
        TestAssembly(a.GetName());
        List<ResourceInfo> ret = [];
        HashSet<string> remainingPreds = new(Comparers.CaseInsensitiveStringComparer);
        while (nodes.Count > 0)
        {
            remainingPreds.Clear();
            nodes.ForEach(n => remainingPreds.Add(n.RI.AssemblyName));
            var nopreds = nodes.Where(n => !n.PredecessorNames.Any(an => remainingPreds.Contains(an))).ToList();
            Debug.Assert(nopreds.Count > 0);
            foreach (var np in nopreds)
            {
                ret.Add(np.RI);
                nodes.Remove(n => n.RI.AssemblyName == np.RI.AssemblyName);
            }
            continue;
        }
        return new ConfigStackSettings { Resources = ret };

        HashSet<string> TestAssembly(AssemblyName an)
        {
            var aname = an?.Name;
            if (an == null || ignoreExpr.IsMatch(aname))
            {
                return [];
            }
            else if (testedAssemblyNames.Contains(aname))
            {
                return [aname];
            }
            testedAssemblyNames.Add(aname);
            Assembly a;
            try
            {
                a = Assembly.Load(an);
            }
            catch (System.IO.FileNotFoundException)
            {
                return [];
            }
            HashSet<string> predecessors = new(Comparers.CaseInsensitiveStringComparer);
            foreach (var kid in a.GetReferencedAssemblies())
            {
                var preds = TestAssembly(kid);
                if (preds != null)
                {
                    predecessors.AddRange(preds);
                }
            }
            var csasa = a.GetCustomAttribute<ConfigStackSettingsAppSettingsAttribute>();
            if (csasa != null)
            {
                nodes.Add(new(predecessors.ToHashSet(), new() { AssemblyName = aname, BaseConfigFileName = csasa.BaseConfigFileName }));
                predecessors.Add(aname);
            }
            return predecessors;
        }
    }

    public List<ResourceInfo> Resources { get; set; }

    public sealed record ConfigStackSettingsConfig(Assembly ConfigSettingsAssembly = null, string ConfigSettingsResourceName = null, bool AutoDiscover = false, string AutoDiscoveryIgnoreAssemblyNamePattern = "^(Microsoft|System)\\.")
    { }

    public static void AutoDiscover(IConfigurationBuilder builder, string environmentName, Assembly configSettingsAssembly = null, ILogger logger = null)
        => Add(builder, environmentName, new ConfigStackSettingsConfig(configSettingsAssembly, null, true), logger);

    public static void Add(IConfigurationBuilder builder, string environmentName, Assembly configSettingsAssembly = null, string configSettingsResourceName = null, ILogger logger = null)
        => Add(builder, environmentName, new ConfigStackSettingsConfig(configSettingsAssembly, configSettingsResourceName), logger);

    public static void Add(IConfigurationBuilder builder, string environmentName, ConfigStackSettingsConfig config = null, ILogger logger = null)
    {
        logger ??= new TraceLoggerProvider().CreateLogger(nameof(ConfigStackSettings));
        config ??= new();
        var a = config.ConfigSettingsAssembly ?? Assembly.GetEntryAssembly();
        ArgumentNullException.ThrowIfNull(a, $"You must either pass in a {nameof(config.ConfigSettingsAssembly)} OR be on a platform where Assembly.GetEntryAssembly() functions");
        ConfigStackSettings css;
        if (config.AutoDiscover)
        {
            css = Discover(config, a, logger);
        }
        else
        {
            var st = a.GetEmbeddedResourceAsStream(config.ConfigSettingsResourceName ?? ConfigSettingsJsonFileName);
            if (st != null)
            {
                builder.AddJsonStream(st);
            }
            var c = builder.Build();
            css = c.Get<ConfigStackSettings>(ConfigSectionName);
        }
        css.Add(builder, environmentName, logger);
    }

    private void Add(IConfigurationBuilder builder, string environmentName, ILogger logger)
    {
        Requires.Text(environmentName);

        var assemblyByName = AssemblyLoadContext.Default.Assemblies.ToDictionary(a => a.GetName().Name, Comparers.CaseInsensitiveStringComparer);

        void AddJsonStream(Assembly a, string name, bool required)
        {
            var st = a.GetEmbeddedResourceAsStream(name);
            if (st != null)
            {
                builder.AddJsonStream(st);
                logger.LogInformation($"Stacking json from assembly {a.GetName().Name} of resource {name}");
            }
            else if (required)
            {
                throw new Exception($"Required json config resource [{name}] not found");
            }
        }

        logger.LogInformation($"AssemblyNames: {assemblyByName.Keys.Format(",", "[{0}]")}");
        logger.LogInformation($"Resources: {Resources.Format(", ")}");

        foreach (var info in Resources)
        {
            var a = assemblyByName.GetValue(info.AssemblyName);
            logger.LogInformation($"Processing {info.AssemblyName} with {info.BaseConfigFileName}");
            if (a == null)
            {
                a = Assembly.Load(info.AssemblyName);
                if (a == null)
                {
                    throw new Exception($"Cannot find assembly {info.AssemblyName}");
                }
            }
            var baseName = info.BaseConfigFileName ?? "appsettings";
            AddJsonStream(a, $"{baseName}.json", true);
            AddJsonStream(a, $"{baseName}.{environmentName}.json", false);
            AddJsonStream(a, $"{baseName}.builder.json", false);
#if DEBUG
            AddJsonStream(a, $"{baseName}.local.json", false);
#endif
        }
    }
}
