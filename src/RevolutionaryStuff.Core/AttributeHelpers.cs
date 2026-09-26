using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Collections;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides helpers for inspecting attributes on types, enum values, and members.
/// </summary>
public static class AttributeStuff
{
    /// <summary>
    /// Determines whether a type has an attribute of the specified type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to find.</typeparam>
    /// <param name="t">The type to inspect.</param>
    /// <param name="inherit">Whether to search the type's inheritance chain.</param>
    /// <returns><see langword="true"/> if the type has the attribute; otherwise, <see langword="false"/>.</returns>
    public static bool HasCustomAttribute<TAttribute>(this Type t, bool inherit = true) where TAttribute : Attribute
    {
        return t.GetCustomAttribute<TAttribute>(inherit) != null;
    }

    /// <summary>
    /// Gets attributes of the specified type applied to an enum value's member.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to retrieve.</typeparam>
    /// <param name="e">The enum value whose member is inspected.</param>
    /// <returns>The matching attributes, or an empty sequence if the enum member cannot be found.</returns>
    public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this Enum e) where TAttribute : Attribute
    {
        var ti = e.GetType().GetTypeInfo();
        var members = ti.GetMember(e.ToString());
        if (members.Length == 0)
        {
            return new TAttribute[0];
        }

        var mi = ti.GetMember(e.ToString())[0];
        return mi.GetCustomAttributes<TAttribute>();
    }

    /// <summary>
    /// Gets the first attribute of the specified type applied to an enum value's member.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to retrieve.</typeparam>
    /// <param name="e">The enum value whose member is inspected.</param>
    /// <returns>The first matching attribute, or <see langword="null"/> if none is present.</returns>
    public static TAttribute GetCustomAttribute<TAttribute>(this Enum e) where TAttribute : Attribute => e.GetCustomAttributes<TAttribute>().FirstOrDefault();

    /// <summary>
    /// Gets the first attribute of the specified type applied to a type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to retrieve.</typeparam>
    /// <param name="t">The type to inspect.</param>
    /// <param name="inherit">Whether to search the type's inheritance chain.</param>
    /// <returns>The first matching attribute, or <see langword="null"/> if none is present.</returns>
    public static TAttribute GetCustomAttribute<TAttribute>(this Type t, bool inherit = true) where TAttribute : Attribute => t.GetCustomAttributes<TAttribute>(inherit).FirstOrDefault();

    /// <summary>
    /// Gets attributes of the specified type applied to a type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to retrieve.</typeparam>
    /// <param name="t">The type to inspect.</param>
    /// <param name="inherit">Whether to search the type's inheritance chain.</param>
    /// <returns>The matching attributes.</returns>
    public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this Type t, bool inherit = true) where TAttribute : Attribute
        => PermaCache.FindOrCreate(
            t, typeof(TAttribute), inherit,
            () => t.GetTypeInfo().GetCustomAttributes(inherit).OfType<TAttribute>().ConvertAll(a => (Attribute)a).AsReadOnly()
            ).OfType<TAttribute>();

    /// <summary>
    /// Gets the members of a type that have an attribute of the specified type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to find.</typeparam>
    /// <param name="t">The type whose members are inspected.</param>
    /// <param name="flags">The binding flags used to select members.</param>
    /// <returns>The members with a matching attribute.</returns>
    public static IEnumerable<MemberInfo> GetAttributedMembers<TAttribute>(this Type t, BindingFlags flags) where TAttribute : Attribute
    {
        foreach (var mi in t.GetMembers(flags))
        {
            if (mi.GetCustomAttribute<TAttribute>() != null) yield return mi;
        }
    }

    /// <summary>
    /// Gets matching attributes applied to exported types in an assembly.
    /// </summary>
    /// <param name="typeAttributeTypes">The attribute types to find.</param>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>A dictionary mapping each exported type to its matching attributes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeAttributeTypes"/> or <paramref name="assembly"/> is <see langword="null"/>.</exception>
    public static MultipleValueDictionary<Type, Attribute> GetAttributesByPublicType(IEnumerable<Type> typeAttributeTypes,
                                                                                Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(typeAttributeTypes);
        var attributesByPublicType = new MultipleValueDictionary<Type, Attribute>(null, () => []);
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
    /// Gets matching attributes applied to exported types in assemblies and assembly files.
    /// </summary>
    /// <param name="typeAttributeTypes">The attribute types to find.</param>
    /// <param name="assemblies">Assemblies to inspect, or <see langword="null"/> to skip this source.</param>
    /// <param name="dllPaths">Paths to assembly files to load and inspect, or <see langword="null"/> to skip this source.</param>
    /// <param name="assemblyFilter">An optional predicate that selects assemblies for inspection.</param>
    /// <returns>
    /// A dictionary mapping each exported type to its matching attributes. Assembly files that cannot be loaded are skipped.
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
            foreach (var dllPath in dllPaths)
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
        ms.ForEach(attributesByPublicType.Add);
        return attributesByPublicType;
    }
}
