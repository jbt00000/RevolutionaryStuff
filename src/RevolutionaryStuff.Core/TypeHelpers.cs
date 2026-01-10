using System.Collections;
using System.Dynamic;
using System.Reflection;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides utility methods for working with types, reflection, and type conversions.
/// </summary>
public static class TypeHelpers
{
    /// <summary>
    /// Determines whether the specified type is a value type or a string.
    /// </summary>
    /// <param name="t">The type to check.</param>
    /// <returns><c>true</c> if the type is a value type or string; otherwise, <c>false</c>.</returns>
    public static bool IsValueTypeOrString(this Type t)
        => t.IsValueType || t == typeof(string);

    /// <summary>
    /// Determines whether the specified type is a nullable enum type.
    /// </summary>
    /// <param name="t">The type to check.</param>
    /// <returns><c>true</c> if the type is a nullable enum; otherwise, <c>false</c>.</returns>
    public static bool IsNullableEnum(this Type t)
    {
        if (t.IsValueType)
        {
            var u = Nullable.GetUnderlyingType(t);
            return u is { IsEnum: true };
        }
        return false;
    }

    /// <summary>
    /// Determines whether the specified type is nullable (either a reference type or a nullable value type).
    /// </summary>
    /// <param name="t">The type to check.</param>
    /// <returns><c>true</c> if the type is nullable; otherwise, <c>false</c>.</returns>
    public static bool IsNullable(this Type t)
    {
        return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    /// <summary>
    /// Gets the default value for the specified type.
    /// </summary>
    /// <param name="t">The type to get the default value for.</param>
    /// <returns>The default value for value types, or <c>null</c> for reference types.</returns>
    public static object GetDefaultValue(this Type t)
    {
        return t.GetTypeInfo().IsValueType ? Activator.CreateInstance(t) : null;
    }

    /// <summary>
    /// Constructs an instance of the specified type using its parameterless constructor.
    /// Special handling for IList&lt;T&gt; and IDictionary&lt;TKey, TValue&gt; interfaces.
    /// </summary>
    /// <param name="t">The type to construct.</param>
    /// <returns>A new instance of the specified type.</returns>
    public static object Construct(this Type t)
    {
        return t.Name == "IList`1"
            ? ConstructList(t.GenericTypeArguments[0])
            : t.Name == "IDictionary`2"
            ? ConstructDictionary(t.GenericTypeArguments[0], t.GenericTypeArguments[1])
            : Activator.CreateInstance(t);
    }

    /// <summary>
    /// Constructs an instance of the specified type using its parameterless constructor.
    /// </summary>
    /// <typeparam name="T">The type to construct.</typeparam>
    /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
    public static T Construct<T>() where T : new()
        => (T)Construct(typeof(T));

    /// <summary>
    /// Constructs a dictionary with the specified key and value types.
    /// </summary>
    /// <param name="keyType">The type of the dictionary keys.</param>
    /// <param name="valType">The type of the dictionary values.</param>
    /// <returns>A new <see cref="IDictionary"/> instance with the specified key and value types.</returns>
    public static IDictionary ConstructDictionary(Type keyType, Type valType)
    {
        var gt = typeof(Dictionary<,>).MakeGenericType([keyType, valType]);
        return (IDictionary)Construct(gt);
    }

    /// <summary>
    /// Constructs a list with the specified item type.
    /// </summary>
    /// <param name="itemType">The type of the list items.</param>
    /// <returns>A new <see cref="IList"/> instance with the specified item type.</returns>
    public static IList ConstructList(Type itemType)
    {
        var gt = typeof(List<>).MakeGenericType([itemType]);
        return (IList)Construct(gt);
    }

    /// <summary>
    /// Gets the indexer property from the specified type.
    /// </summary>
    /// <param name="itemType">The type to search for an indexer.</param>
    /// <returns>The indexer <see cref="PropertyInfo"/> if found; otherwise, <c>null</c>.</returns>
    public static PropertyInfo GetIndexer(this Type itemType)
        => itemType.GetProperties().SingleOrDefault(pi => pi.GetIndexParameters().Length == 1);

    /// <summary>
    /// Converts an object to a dictionary of property names and values.
    /// </summary>
    /// <param name="o">The object to convert.</param>
    /// <returns>
    /// A dictionary containing the public instance property names and values.
    /// Returns an empty dictionary if the object is <c>null</c>.
    /// </returns>
    public static IDictionary<string, object> ToPropertyValueDictionary(object o)
    {
        if (o != null)
        {
            if (o is ExpandoObject)
            {
                return (ExpandoObject)o;
            }
            IDictionary<string, object> d = null;
            foreach (var prop in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                d ??= new Dictionary<string, object>();
                d[prop.Name] = prop.GetValue(o, null);
            }
            if (d != null) return d.AsReadOnlyDictionary();
        }
        return Empty.StringObjectDictionary;
    }

    /// <summary>
    /// Determines whether the specified type is a whole number type.
    /// Supports: byte, sbyte, short, ushort, int, uint, long, ulong, nint, nuint, Int128, UInt128.
    /// </summary>
    /// <param name="t">The type to check.</param>
    /// <returns><c>true</c> if the type is a whole number; otherwise, <c>false</c>.</returns>
    public static bool IsWholeNumber(this Type t)
    {
        return
                   t == typeof(short) ||
                   t == typeof(int) ||
                   t == typeof(long) ||
                   t == typeof(ushort) ||
                   t == typeof(uint) ||
                   t == typeof(ulong) ||
                   t == typeof(sbyte) ||
                   t == typeof(byte) ||
                   t == typeof(nint) ||
                   t == typeof(nuint) ||
                   t == typeof(Int128) ||
                   t == typeof(UInt128);
    }

    /// <summary>
    /// Determines whether the specified type is a real (floating-point) number type.
    /// Supports: float, double, decimal, Half.
    /// </summary>
    /// <param name="t">The type to check.</param>
    /// <returns><c>true</c> if the type is a real number; otherwise, <c>false</c>.</returns>
    public static bool IsRealNumber(this Type t)
    {
        return
                   t == typeof(float) ||
                   t == typeof(double) ||
                   t == typeof(decimal) ||
                   t == typeof(Half);
    }

    /// <summary>
    /// Determines whether the specified type is a numeric type (whole number or real number).
    /// </summary>
    /// <param name="t">The type to check.</param>
    /// <returns><c>true</c> if the type is numeric; otherwise, <c>false</c>.</returns>
    public static bool IsNumber(this Type t)
    {
        return IsWholeNumber(t) || IsRealNumber(t);
    }

    /// <summary>
    /// Gets the maximum and minimum values for the specified numeric type.
    /// </summary>
    /// <param name="t">The numeric type to get bounds for.</param>
    /// <param name="max">When this method returns, contains the maximum value for the type.</param>
    /// <param name="min">When this method returns, contains the minimum value for the type.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the type is not numeric.</exception>
    public static void NumericMaxMin(Type t, out double max, out double min)
    {
        max = min = 0;
        if (t == typeof(short))
        {
            max = short.MaxValue;
            min = short.MinValue;
        }
        else if (t == typeof(int))
        {
            max = int.MaxValue;
            min = int.MinValue;
        }
        else if (t == typeof(long))
        {
            max = long.MaxValue;
            min = long.MinValue;
        }
        else if (t == typeof(ushort))
        {
            max = ushort.MaxValue;
            min = ushort.MinValue;
        }
        else if (t == typeof(uint))
        {
            max = uint.MaxValue;
            min = uint.MinValue;
        }
        else if (t == typeof(ulong))
        {
            max = ulong.MaxValue;
            min = ulong.MinValue;
        }
        else if (t == typeof(sbyte))
        {
            max = sbyte.MaxValue;
            min = sbyte.MinValue;
        }
        else if (t == typeof(byte))
        {
            max = byte.MaxValue;
            min = byte.MinValue;
        }
        else if (t == typeof(float))
        {
            max = float.MaxValue;
            min = float.MinValue;
        }
        else if (t == typeof(double))
        {
            max = double.MaxValue;
            min = double.MinValue;
        }
        else if (t == typeof(decimal))
        {
            max = Convert.ToDouble(decimal.MaxValue);
            min = Convert.ToDouble(decimal.MinValue);
        }
        else if (t == typeof(Half))
        {
            max = (double)Half.MaxValue;
            min = (double)Half.MinValue;
        }
        else if (t == typeof(nint))
        {
            max = nint.MaxValue;
            min = nint.MinValue;
        }
        else if (t == typeof(nuint))
        {
            max = nuint.MaxValue;
            min = nuint.MinValue;
        }
        else if (t == typeof(Int128))
        {
            // Int128 is too large for double, use max double value as approximation
            max = double.MaxValue;
            min = double.MinValue;
        }
        else if (t == typeof(UInt128))
        {
            // UInt128 is too large for double, use max double value as approximation
            max = double.MaxValue;
            min = 0;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(t), $"{t?.Name} was not numeric");
        }
    }

    private static void RequiresIsPropertyInfoOrFieldInfo(MemberInfo mi)
    {
        ArgumentNullException.ThrowIfNull(mi);
        if (mi is PropertyInfo or FieldInfo) return;
        throw new Exception($"we weren't expectecting a {mi.GetType()}");
    }

    /// <summary>
    /// Gets the underlying type of a property or field member.
    /// </summary>
    /// <param name="mi">The member to get the underlying type from.</param>
    /// <returns>The property type or field type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mi"/> is <c>null</c>.</exception>
    /// <exception cref="Exception">Thrown when <paramref name="mi"/> is neither a PropertyInfo nor a FieldInfo.</exception>
    public static Type GetUnderlyingType(this MemberInfo mi)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        return mi is PropertyInfo pi ? pi.PropertyType : mi is FieldInfo fi ? fi.FieldType : null;
    }

    /// <summary>
    /// Gets the value of a property or field from the specified object.
    /// </summary>
    /// <param name="mi">The property or field member.</param>
    /// <param name="basevar">The object to get the value from.</param>
    /// <returns>The value of the property or field.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mi"/> is <c>null</c>.</exception>
    /// <exception cref="Exception">Thrown when <paramref name="mi"/> is neither a PropertyInfo nor a FieldInfo.</exception>
    public static object GetValue(this MemberInfo mi, object basevar)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        return mi is PropertyInfo pi
            ? pi.GetValue(basevar, null)
            : mi is FieldInfo fi ? fi.GetValue(basevar) : throw new UnexpectedSwitchValueException(mi.GetType().Name);
    }

    /// <summary>
    /// Sets the value of a property or field on the specified object, converting the value if necessary.
    /// </summary>
    /// <param name="mi">The property or field member.</param>
    /// <param name="basevar">The object to set the value on.</param>
    /// <param name="val">The value to set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mi"/> is <c>null</c>.</exception>
    /// <exception cref="Exception">Thrown when <paramref name="mi"/> is neither a PropertyInfo nor a FieldInfo.</exception>
    public static void SetValue(this MemberInfo mi, object basevar, object val)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        var v = mi.ConvertValue(val);
        if (mi is PropertyInfo pi)
        {
            pi.SetValue(basevar, v, null);
        }
        else if (mi is FieldInfo fi)
        {
            fi.SetValue(basevar, v);
        }
        else
        {
            throw new UnexpectedSwitchValueException(mi.GetType().Name);
        }
    }

    /// <summary>
    /// Converts a value to the type of the specified property or field member.
    /// </summary>
    /// <param name="mi">The property or field member whose type to convert to.</param>
    /// <param name="val">The value to convert.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mi"/> is <c>null</c>.</exception>
    /// <exception cref="Exception">Thrown when <paramref name="mi"/> is neither a PropertyInfo nor a FieldInfo.</exception>
    public static object ConvertValue(this MemberInfo mi, object val)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        return mi is PropertyInfo pi
            ? ConvertValue(pi.PropertyType, val)
            : mi is FieldInfo fi ? ConvertValue(fi.FieldType, val) : throw new UnexpectedSwitchValueException(mi.GetType().Name);
    }

    /// <summary>
    /// Converts a string value to the specified type.
    /// </summary>
    /// <param name="t">The target type.</param>
    /// <param name="val">The string value to convert.</param>
    /// <returns>The converted value.</returns>
    public static object ConvertValue(Type t, string val)
    {
        var ti = t.GetTypeInfo();
        if (ti.IsGenericType && ti.GenericTypeArguments.Length == 1 && ti.Name == "Nullable`1" && ti.Namespace == "System")
        {
            if (val == null) return null;
            t = ti.GenericTypeArguments[0];
            ti = t.GetTypeInfo();
        }
        try
        {
            if (t == typeof(TimeSpan))
            {
                return TimeSpan.Parse(val);
            }

            if (t == typeof(bool))
            {
                return Parse.ParseBool(val);
            }
            if (t == typeof(Uri))
            {
                return new Uri(val);
            }
            if (ti.IsEnum)
            {
                return Enum.Parse(t, val, true);
            }
        }
        catch (Exception)
        { }
        return Convert.ChangeType(val, t);
    }

    /// <summary>
    /// Converts an object value to the specified type.
    /// </summary>
    /// <param name="t">The target type.</param>
    /// <param name="val">The value to convert.</param>
    /// <returns>The converted value, or <c>null</c> if the input value is <c>null</c>.</returns>
    public static object ConvertValue(Type t, object val)
    {
        object v = null;
        if (val == null)
        {
            Stuff.NoOp();
        }
        else
        {
            var ti = t.GetTypeInfo();
            v = ti.IsAssignableFrom(val.GetType())
                ? val
                : ti.IsEnum
                    ? Enum.Parse(t, val.ToString(), true)
                    : ti.IsGenericType &&
                                                t.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                                                t.GenericTypeArguments.Length == 1 &&
                                                t.GenericTypeArguments[0].GetTypeInfo().IsEnum
                                    ? Enum.Parse(t.GenericTypeArguments[0], val.ToString(), true)
                                    : ChangeType(val, t);
        }
        return v;
    }

    private static object ChangeType(object val, Type t)
    {
        if (t.FullName == "System.Object") return val;
        if (t == typeof(bool))
        {
            var sval = StringHelpers.ToString(val);
            return Parse.TryParseBool(sval, out var b) ? (object)b : throw new NotSupportedException($"ChangeType could change [{val}] into a bool.");
        }

        return t.GetTypeInfo().IsEnum
            ? Enum.Parse(t, val.ToString(), true)
            : t == typeof(Uri) && val is string ? new Uri((string)val) : Convert.ChangeType(val, t, null);
    }

    /// <summary>
    /// Determines whether a property or field can be written to.
    /// </summary>
    /// <param name="mi">The property or field member to check.</param>
    /// <returns>
    /// <c>true</c> if the member is a writable property or a field that is not readonly or const;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mi"/> is <c>null</c>.</exception>
    /// <exception cref="Exception">Thrown when <paramref name="mi"/> is neither a PropertyInfo nor a FieldInfo.</exception>
    public static bool CanWrite(this MemberInfo mi)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        return mi is PropertyInfo pi ? pi.CanWrite : mi is FieldInfo fi && !(fi.IsInitOnly || fi.IsLiteral);
    }

    /// <summary>
    /// Gets assembly attribute information for the specified assembly.
    /// </summary>
    /// <param name="a">The assembly to get information from.</param>
    /// <returns>An <see cref="AssemblyAttributeInfo"/> instance containing assembly metadata.</returns>
    public static AssemblyAttributeInfo GetInfo(this Assembly a)
    {
        return new AssemblyAttributeInfo(a);
    }

    /// <summary>
    /// Gets all assemblies loaded in the current application domain, including their dependencies.
    /// </summary>
    /// <returns>An enumerable collection of all loaded assemblies.</returns>
    public static IEnumerable<Assembly> GetLoadedAssemblies()
    {
        var tested = new HashSet<string>();
        var d = new Dictionary<string, Assembly>();
        var entry = Assembly.GetEntryAssembly();
        d[entry.GetName().ToString()] = entry;
Again:
        foreach (var kvp in d)
        {
            if (tested.Contains(kvp.Key)) continue;
            tested.Add(kvp.Key);
            foreach (var an in kvp.Value.GetReferencedAssemblies())
            {
                if (d.ContainsKey(an.ToString())) continue;
                d[an.ToString()] = Assembly.Load(an);

            }
            goto Again;
        }
        return [.. d.Values];
    }

    /// <summary>
    /// Gets the public parameterless constructor for the specified type.
    /// </summary>
    /// <param name="test">The type to search.</param>
    /// <returns>The <see cref="ConstructorInfo"/> for the parameterless constructor, or <c>null</c> if not found.</returns>
    public static ConstructorInfo GetConstructorNoParameters(this Type test)
        => test.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(ci => ci.GetParameters().Length == 0);

    /// <summary>
    /// Gets all public instance properties that can be read.
    /// </summary>
    /// <param name="test">The type to search.</param>
    /// <returns>An array of public readable instance properties.</returns>
    public static PropertyInfo[] GetPropertiesPublicInstanceRead(this Type test)
        => test.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

    /// <summary>
    /// Gets all public instance properties that can be both read and written.
    /// </summary>
    /// <param name="test">The type to search.</param>
    /// <returns>An array of public read/write instance properties.</returns>
    public static PropertyInfo[] GetPropertiesPublicInstanceReadWrite(this Type test)
        => test.GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

    /// <summary>
    /// Gets all public instance fields that can be read.
    /// </summary>
    /// <param name="test">The type to search.</param>
    /// <returns>An array of public readable instance fields.</returns>
    public static FieldInfo[] GetFieldsPublicInstanceRead(this Type test)
        => test.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);

    /// <summary>
    /// Gets all public instance fields that can be both read and written.
    /// </summary>
    /// <param name="test">The type to search.</param>
    /// <returns>An array of public read/write instance fields.</returns>
    public static FieldInfo[] GetFieldsPublicInstanceReadWrite(this Type test)
        => test.GetFields(BindingFlags.SetField | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);

    /// <summary>
    /// Determines whether the specified type is assignable to the generic type parameter.
    /// </summary>
    /// <typeparam name="T">The potential base type.</typeparam>
    /// <param name="test">The type to check.</param>
    /// <returns><c>true</c> if <paramref name="test"/> is assignable to <typeparamref name="T"/>; otherwise, <c>false</c>.</returns>
    public static bool IsA<T>(this Type test)
        => IsA(test, typeof(T));

    /// <summary>
    /// Determines whether the specified type is assignable to the potential base type.
    /// </summary>
    /// <param name="test">The type to check.</param>
    /// <param name="potentialBaseType">The potential base type.</param>
    /// <returns><c>true</c> if <paramref name="test"/> is assignable to <paramref name="potentialBaseType"/>; otherwise, <c>false</c>.</returns>
    public static bool IsA(this Type test, Type potentialBaseType)
    {
        return potentialBaseType.GetTypeInfo().IsAssignableFrom(test);
    }

    /// <summary>
    /// Recursively walks through all members of a type using reflection, invoking callbacks at each node.
    /// </summary>
    /// <typeparam name="TNodeContext">The type of context object passed through the walk.</typeparam>
    /// <param name="baseType">The root type to start walking from.</param>
    /// <param name="bindingFlags">The binding flags to use when getting members.</param>
    /// <param name="createContext">Callback to create a new context for each node.</param>
    /// <param name="visit">Callback invoked for each member visited.</param>
    /// <param name="recurse">Optional callback to determine if recursion should continue for a member. Defaults to always recurse.</param>
    /// <param name="parentNodeContext">The parent node's context.</param>
    /// <param name="depth">The current depth in the walk (starts at 0).</param>
    /// <param name="loopChecker">Internal set used to prevent infinite recursion.</param>
    /// <param name="baseTypeMemberInfo">The member info of the parent type.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseType"/> or <paramref name="visit"/> is <c>null</c>.</exception>
    public static void MemberWalk<TNodeContext>(
        this Type baseType,
        BindingFlags bindingFlags,
        Func<TNodeContext, Type, MemberInfo, TNodeContext> createContext,
        Action<TNodeContext, Type, MemberInfo> visit,
        Func<TNodeContext, Type, MemberInfo, bool> recurse = null,
        TNodeContext parentNodeContext = default,
        int depth = 0,
        HashSet<Type> loopChecker = null,
        MemberInfo baseTypeMemberInfo = null)
    {
        ArgumentNullException.ThrowIfNull(baseType);
        ArgumentNullException.ThrowIfNull(visit);

        recurse ??= delegate (TNodeContext a, Type b, MemberInfo c) { return true; };
        var nodeContext = createContext(parentNodeContext, baseType, baseTypeMemberInfo);

        loopChecker ??= [];
        if (loopChecker.Contains(baseType)) return;
        loopChecker.Add(baseType);
        foreach (var mi in baseType.GetMembers(bindingFlags))
        {
            var memberType = (mi as PropertyInfo)?.PropertyType ?? (mi as FieldInfo)?.FieldType;
            if (memberType != null)
            {
                visit(nodeContext, memberType, mi);
                var mti = memberType.GetTypeInfo();
                if (memberType.IsNumber() || memberType == typeof(string) || !mti.IsClass) { }
                else if (recurse(nodeContext, memberType, mi))
                {
                    MemberWalk(memberType, bindingFlags, createContext, visit, recurse, nodeContext, depth + 1, loopChecker, mi);
                }
            }
        }
    }
}
