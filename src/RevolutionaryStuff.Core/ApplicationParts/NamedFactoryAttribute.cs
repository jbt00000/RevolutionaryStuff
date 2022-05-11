using System.IO;
using System.Reflection;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core.ApplicationParts;

[AttributeUsage(AttributeTargets.Class)]
public class NamedFactoryAttribute : Attribute
{
    public string FactoryName;

    public NamedFactoryAttribute(string factoryName)
    {
        FactoryName = factoryName;
    }

    public static ICollection<Type> Find(Predicate<string> factoryNameFilter, Type interfaceType = null)
    {
        factoryNameFilter ??= delegate (string z) { return true; };
        return Cache.DataCacher.FindOrCreateValue(
            Cache.CreateKey(typeof(NamedFactoryAttribute), nameof(Find), factoryNameFilter, interfaceType == null ? "" : interfaceType.AssemblyQualifiedName),
            delegate ()
            {
                var m = AttributeStuff.GetAttributesByPublicType(
                    new[] { typeof(NamedFactoryAttribute) },
                    TypeHelpers.GetLoadedAssemblies(),
                    Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "*.dll", SearchOption.TopDirectoryOnly),
                    test => test.GetCustomAttribute<PluginDomainAttribute>() != null);
                var ret = new List<Type>();
                foreach (var kvp in m.AtomEnumerable)
                {
                    var nfa = (NamedFactoryAttribute)kvp.Value;
                    if (!factoryNameFilter(nfa.FactoryName)) continue;
                    if (interfaceType == null)
                    {
                        ret.Add(kvp.Key);
                    }
                    else
                    {
                        var matches = kvp.Key.GetTypeInfo().FindInterfaces(delegate (Type typeObj, object criteria)
                        {
                            return typeObj.IsA(interfaceType);
                        }, null);
                        if (matches.Length > 0)
                        {
                            ret.Add(kvp.Key);
                        }
                    }
                }
                return ret;
            });
    }

    public static ICollection<Type> Find<I>(string factoryName) where I : class
    {
        return Find(z => z == factoryName, typeof(I));
    }

    public static I InstantiateFactory<I>(string factoryName) where I : class
    {
        var types = Find<I>(factoryName);
        var t = types.Single();
        var factoryConstructorInfo = t.GetTypeInfo().GetConstructor(Empty.TypeArray);
        var factory = (I)factoryConstructorInfo.Invoke(Empty.ObjectArray);
        return factory;
    }

    public static ICollection<I> InstantiateFactories<I>(string factoryNamesCsv) where I : class
    {
        return InstantiateFactories<I>(CSV.ParseLine(factoryNamesCsv ?? ""));
    }

    public static ICollection<I> InstantiateFactories<I>(IEnumerable<string> factoryNames) where I : class
    {
        var filter = (factoryNames ?? Empty.StringArray).ToSet();
        return InstantiateFactories<I>(name => filter.Contains(name));
    }

    public static ICollection<I> InstantiateFactories<I>(Predicate<string> factoryNameFilter = null) where I : class
    {
        var factories = new List<I>();
        var types = Find(factoryNameFilter, typeof(I));
        foreach (var t in types)
        {
            var factory = Cache.DataCacher.FindOrCreateValue(
                Cache.CreateKey(typeof(NamedFactoryAttribute), nameof(InstantiateFactories), t, nameof(InstantiateFactories)),
                () =>
                {
                    var factoryConstructorInfo = t.GetTypeInfo().GetConstructor(Empty.TypeArray);
                    return factoryConstructorInfo.Invoke(Empty.ObjectArray);
                });
            factories.Add((I)factory);
        }
        return factories;
    }
}
