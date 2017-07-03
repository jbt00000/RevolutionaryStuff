using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NamedFactoryAttribute : Attribute
    {
        public string FactoryName;

        public NamedFactoryAttribute(string factoryName)
        {
            FactoryName = factoryName;
        }

        private static ICache<string, ICollection<Type>> FindCache = Cache.CreateSynchronized<string, ICollection<Type>>();

        public static ICollection<Type> Find(Predicate<string> factoryNameFilter, Type interfaceType = null)
        {
            factoryNameFilter = factoryNameFilter ?? delegate (string z) { return true; };
            return FindCache.Do(
                Cache.CreateKey(factoryNameFilter, interfaceType == null ? "" : interfaceType.AssemblyQualifiedName),
                delegate ()
                {
                    var m = AttributeStuff.GetAttributesByPublicType(
                        new[] { typeof(NamedFactoryAttribute) },
                        TypeHelpers.GetLoadedAssemblies(),
                        Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "*.dll", SearchOption.TopDirectoryOnly),
                        test=>test.GetCustomAttribute<PluginDomainAttribute>()!=null);
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
            return NamedFactoryAttribute.Find(z=>z==factoryName, typeof(I));
        }

        public static I InstantiateFactory<I>(string factoryName) where I : class
        {
            var types = NamedFactoryAttribute.Find<I>(factoryName);
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

        private static readonly ICache<Type, object> FactoryCache = Cache.CreateSynchronized<Type, object>();

        public static ICollection<I> InstantiateFactories<I>(Predicate<string> factoryNameFilter=null) where I : class
        {
            var factories = new List<I>();
            var types = Find(factoryNameFilter, typeof(I));
            foreach (var t in types)
            {
                var factory = FactoryCache.Do(t, ()=>
                {
                    var factoryConstructorInfo = t.GetTypeInfo().GetConstructor(Empty.TypeArray);
                    return factoryConstructorInfo.Invoke(Empty.ObjectArray);
                });
                factories.Add((I)factory);
            }
            return factories;
        }
    }
}
