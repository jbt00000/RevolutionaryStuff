using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core.ApplicationParts;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class AssemblySettingsResourceAutoDiscoveryAttribute : Attribute
{
    public readonly string BaseResourceName;

    public AssemblySettingsResourceAutoDiscoveryAttribute(string baseResourceName = null)
    {
        BaseResourceName = baseResourceName;
    }
}

public static class AssemblySettingsResourceStacking
{
    private const string DefaultBaseResourceName = "appsettings";

    public record ResourceInfo(Assembly Assembly, string BaseResourceName = null)
    {
        public override string ToString()
            => $"assembly=>{Assembly.GetName()} baseResourceName=>{BaseResourceName}";
    }

    private static readonly ApplyOptions DefaultApplyOptions = new(null);

    private const string DefaultIgnoreAssemblyNamePattern = "^(Azure|Microsoft|System)\\.";

    public record DiscoverOptions(string IgnoreAssemblyNamePattern) { }

    public static IEnumerable<ResourceInfo> Discover(Assembly a, DiscoverOptions discoverOptions = null, ILogger logger = null)
    {
        ArgumentNullException.ThrowIfNull(a);
        logger ??= Stuff.LoggerOfLastResort;
        List<(HashSet<string> PredecessorNames, ResourceInfo RI)> nodes = [];
        var ignoreExpr = new Regex(discoverOptions?.IgnoreAssemblyNamePattern ?? DefaultIgnoreAssemblyNamePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        HashSet<string> testedAssemblyNames = [];
        TestAssembly(a.GetName());
        List<ResourceInfo> ret = [];
        HashSet<string> remainingPreds = new(Comparers.CaseInsensitiveStringComparer);
        while (nodes.Count > 0)
        {
            remainingPreds.Clear();
            nodes.ForEach(n => remainingPreds.Add(n.RI.Assembly.GetName().Name));
            var nopreds = nodes.Where(n => !n.PredecessorNames.Any(an => remainingPreds.Contains(an))).ToList();
            Debug.Assert(nopreds.Count > 0);
            foreach (var np in nopreds)
            {
                ret.Add(np.RI);
                nodes.Remove(n => n.RI.Assembly == np.RI.Assembly);
            }
            continue;
        }
        return ret;

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
            var csasa = a.GetCustomAttribute<AssemblySettingsResourceAutoDiscoveryAttribute>();
            if (csasa != null)
            {
                nodes.Add(new(predecessors.ToHashSet(), new(a, csasa.BaseResourceName)));
                predecessors.Add(aname);
            }
            return predecessors;
        }
    }
    public record ApplyOptions(string DefaultBaseResourceName) { }

    public static void DiscoverThenStack(this IConfigurationBuilder builder, string environmentName, Assembly a, DiscoverOptions discoverOptions = null, ILogger logger = null, ApplyOptions applyOptions = null)
    {
        var items = Discover(a, discoverOptions, logger);
        builder.Stack(environmentName, items, logger, applyOptions);
    }

    public static void Stack(this IConfigurationBuilder builder, string environmentName, IEnumerable<ResourceInfo> resourceInfos, ILogger logger = null, ApplyOptions applyOptions = null)
    {
        const string LoggingPrefix = $"{nameof(AssemblySettingsResourceStacking)}.{nameof(Stack)}: ";
        ArgumentNullException.ThrowIfNull(builder);
        Requires.Text(environmentName);
        logger ??= Stuff.LoggerOfLastResort;
        if (!resourceInfos.NullSafeAny())
        {
            logger.LogWarning($"{LoggingPrefix} No resourceInfos were received. Resource stacking will not take place");
            return;
        }
        applyOptions ??= DefaultApplyOptions;
        var baseResourceName = applyOptions.DefaultBaseResourceName ?? DefaultBaseResourceName;
        logger.LogDebug($"{LoggingPrefix} Will use a baseResourceName=[{baseResourceName}] with environment={environmentName}");
        foreach (var ri in resourceInfos)
        {
            var baseName = ri.BaseResourceName ?? baseResourceName;
            logger.LogDebug($"{LoggingPrefix} stacking {ri} with {baseName}");
            AddJsonStream(ri.Assembly, $"{baseName}.json", true);
            AddJsonStream(ri.Assembly, $"{baseName}.{environmentName}.json", false);
            AddJsonStream(ri.Assembly, $"{baseName}.builder.json", false);
        }

        void AddJsonStream(Assembly a, string name, bool required)
        {
            var st = a.GetEmbeddedResourceAsStream(name);
            if (st != null)
            {
                builder.AddJsonStream(st);
                logger.LogInformation($"{LoggingPrefix} Stacking json from assembly {a.GetName().Name} of resource {name}");
            }
            else if (required)
            {
                throw new Exception($"{LoggingPrefix} Required json config resource [{name}] not found");
            }
        }
    }
}
