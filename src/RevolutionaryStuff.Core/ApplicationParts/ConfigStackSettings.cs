using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace RevolutionaryStuff.Core.ApplicationParts;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ConfigStackSettingsAppSettingsAttribute : Attribute
{
    public readonly string BaseConfigFileName;

    public ConfigStackSettingsAppSettingsAttribute(string baseConfigFileName=null)
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

    public static ConfigStackSettings Discover(ConfigStackSettingsConfig config, Assembly a)
    {
        List<(HashSet<string> PredecessorNames, ResourceInfo RI)> nodes = new();
        var ignoreExpr = new Regex(config.AutoDiscoveryIgnoreAssemblyNamePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        HashSet<string> testedAssemblyNames = new();
        TestAssembly(a.GetName());
        List<ResourceInfo> ret = new();
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
            var a = Assembly.Load(an);
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

    public static void AutoDiscover(IConfigurationBuilder builder, string environmentName, Assembly configSettingsAssembly = null)
        => Add(builder, environmentName, new ConfigStackSettingsConfig(configSettingsAssembly, null, true));

    public static void Add(IConfigurationBuilder builder, string environmentName, Assembly configSettingsAssembly = null, string configSettingsResourceName = null)
        => Add(builder, environmentName, new ConfigStackSettingsConfig(configSettingsAssembly, configSettingsResourceName));

    public static void Add(IConfigurationBuilder builder, string environmentName, ConfigStackSettingsConfig config=null)
    {
        config ??= new();
        var a = config.ConfigSettingsAssembly ?? Assembly.GetEntryAssembly();
        ArgumentNullException.ThrowIfNull(a, $"You must either pass in a {nameof(config.ConfigSettingsAssembly)} OR be on a platform where Assembly.GetEntryAssembly() functions");
        ConfigStackSettings css;
        if (config.AutoDiscover)
        {
            css = Discover(config, a);
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
        css.Add(builder, environmentName);
    }

    private void Add(IConfigurationBuilder builder, string environmentName)
    {
        Requires.Text(environmentName);

        var assemblyByName = AssemblyLoadContext.Default.Assemblies.ToDictionary(a => a.GetName().Name, Comparers.CaseInsensitiveStringComparer);

        void AddJsonStream(Assembly a, string name, bool required)
        {
            var st = a.GetEmbeddedResourceAsStream(name);
            if (st != null)
            {
                builder.AddJsonStream(st);
                Trace.WriteLine($"{nameof(ConfigStackSettings)}: Stacking json from assembly {a.GetName().Name} of resource {name}");
            }
            else if (required)
            {
                throw new Exception($"{nameof(ConfigStackSettings)}: Required json config resource [{name}] not found");
            }
        }

        foreach (var info in Resources)
        {
            var a = assemblyByName.GetValue(info.AssemblyName);
            if (a == null)
            {
                a = Assembly.Load(info.AssemblyName);
                if (a == null)
                {
                    throw new Exception($"{nameof(ConfigStackSettings)}: cannot find assembly {info.AssemblyName}");
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
