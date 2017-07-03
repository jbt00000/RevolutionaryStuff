using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RevolutionaryStuff.Core.Collections;
using RevolutionaryStuff.Core.Caching;
using System.Runtime.Loader;

namespace RevolutionaryStuff.Core
{
    public static class AttributeStuff
    {
        private static readonly ICache<string, ICollection<Attribute>> GetCustomAttributeCache =
            Cache.CreateSynchronized<string, ICollection<Attribute>>();


        public static bool HasCustomAttribute<TAttribute>(this Type t, bool inherit = true) where TAttribute : Attribute
        {
            return t.GetCustomAttribute<TAttribute>(inherit) != null;
        }

        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this Enum e) where TAttribute : Attribute
        {
            var ti = e.GetType().GetTypeInfo();
            var mi = ti.GetMember(e.ToString())[0];
            return mi.GetCustomAttributes<TAttribute>();
        }

        public static TAttribute GetCustomAttribute<TAttribute>(this Enum e) where TAttribute : Attribute => e.GetCustomAttributes<TAttribute>().FirstOrDefault();

        public static TAttribute GetCustomAttribute<TAttribute>(this Type t, bool inherit = true) where TAttribute : Attribute => t.GetCustomAttributes<TAttribute>(inherit).FirstOrDefault();

        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this Type t, bool inherit = true) where TAttribute : Attribute
        {
            return GetCustomAttributeCache.Do(
                Cache.CreateKey(t, typeof(TAttribute), inherit),
                () =>
                {
                    return t.GetTypeInfo().GetCustomAttributes(inherit).OfType<TAttribute>().ConvertAll(a => (Attribute)a).AsReadOnly();
                }).OfType<TAttribute>();
        }

        public static IEnumerable<MemberInfo> GetAttributedMembers<TAttribute>(this Type t, BindingFlags flags) where TAttribute : Attribute
        {
            foreach (var mi in t.GetMembers(flags))
            {
                if (mi.GetCustomAttribute<TAttribute>() != null) yield return mi;
            }
        }

        /// <summary>
        /// Gets the set of plugins contained in the given assembly
        /// </summary>
        /// <param name="typeAttributeTypes">The type of attribute we are expecting</param>
        /// <param name="assembly">The assembly in question</param>
        /// <returns>A dictionary.  The key is the Type, The value is the list of attributes of typeAttributeType</returns>
        public static MultipleValueDictionary<Type, Attribute> GetAttributesByPublicType(IEnumerable<Type> typeAttributeTypes,
                                                                                    Assembly assembly)
        {
            Requires.NonNull(assembly, nameof(assembly));
            Requires.NonNull(typeAttributeTypes, nameof(typeAttributeTypes));
            var attributesByPublicType = new MultipleValueDictionary<Type, Attribute>(null, () => new HashSet<Attribute>());
            foreach (var t in assembly.GetExportedTypes())
            {
                var ti = t.GetTypeInfo();
                foreach (var typeAttributeType in typeAttributeTypes)
                {
                    var attrs = ti.GetCustomAttributes(typeAttributeType, true);
                    foreach (Attribute attr in attrs)
                    {
                        attributesByPublicType.Add(t, attr);
                    }
                }
            }
            return attributesByPublicType;
        }

        /// <summary>
        /// Gets the set of plugins contained in the given assemblies and flls
        /// </summary>
        /// <param name="typeAttributeTypes">The type of attribute we are expecting</param>
        /// <param name="assemblies">The list of assemblies to test, may be null</param>
        /// <param name="dllPaths">The list of full paths to dlls to test, may be null</param>
        /// <param name="testDllsInSeparateAppDomains">Should we test the dlls in a separate app domain?</param>
        /// <param name="loadDllsInSeparateAppDomains">Should we load the dlls in a separate app domain?</param>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <returns>
        /// A dictionary.  The key is the Type, The value is the list of attributes of typeAttributeType
        /// </returns>
        public static MultipleValueDictionary<Type, Attribute> GetAttributesByPublicType(
            IEnumerable<Type> typeAttributeTypes,
            IEnumerable<Assembly> assemblies,
            IEnumerable<string> dllPaths,
            Predicate<Assembly> assemblyFilter
            )
        {
            var ms = new List<MultipleValueDictionary<Type, Attribute>>();
            var testedAssemblyNames = new HashSet<string>();

            if (assemblies != null)
            {
                foreach (var a in assemblies)
                {
                    testedAssemblyNames.Add(a.FullName);
                    if (null == assemblyFilter || assemblyFilter(a))
                    {
                        ms.Add(GetAttributesByPublicType(typeAttributeTypes, a));
                    }
                }
            }

            if (dllPaths != null)
            {
                foreach (string dllPath in dllPaths)
                {
                    if (!File.Exists(dllPath)) continue;
                    try
                    {
                        var a = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                        if (testedAssemblyNames.Contains(a.FullName)) continue;
                        if (assemblyFilter != null && !assemblyFilter(a)) continue;
                        testedAssemblyNames.Add(a.FullName);
                        var m = GetAttributesByPublicType(typeAttributeTypes, a);
                        ms.Add(m);
                    }
                    catch (FileLoadException)
                    {
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            var attributesByPublicType = new MultipleValueDictionary<Type, Attribute>();
            ms.ForEach(m => attributesByPublicType.Add(m));
            return attributesByPublicType;
        }
    }
}