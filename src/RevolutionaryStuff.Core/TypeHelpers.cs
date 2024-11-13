using System.Collections;
using System.Dynamic;
using System.Reflection;

namespace RevolutionaryStuff.Core;

public static class TypeHelpers
{
    public static bool IsValueTypeOrString(this Type t)
        => t.IsValueType || t == typeof(string);

    public static bool IsNullableEnum(this Type t)
    {
        if (t.IsValueType)
        {
            var u = Nullable.GetUnderlyingType(t);
            return u is { IsEnum: true };
        }
        return false;
    }

    public static bool IsNullable(this Type t)
    {
        return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    public static object GetDefaultValue(this Type t)
    {
        return t.GetTypeInfo().IsValueType ? Activator.CreateInstance(t) : null;
    }

    public static object Construct(this Type t)
    {
        return t.Name == "IList`1"
            ? ConstructList(t.GenericTypeArguments[0])
            : t.Name == "IDictionary`2"
            ? ConstructDictionary(t.GenericTypeArguments[0], t.GenericTypeArguments[1])
            : Activator.CreateInstance(t);
    }

    public static T Construct<T>() where T : new()
        => (T)Construct(typeof(T));

    public static IDictionary ConstructDictionary(Type keyType, Type valType)
    {
        var gt = typeof(Dictionary<,>).MakeGenericType([keyType, valType]);
        return (IDictionary)Construct(gt);
    }

    public static IList ConstructList(Type itemType)
    {
        var gt = typeof(List<>).MakeGenericType([itemType]);
        return (IList)Construct(gt);
    }

    public static PropertyInfo GetIndexer(this Type itemType)
        => itemType.GetProperties().SingleOrDefault(pi => pi.GetIndexParameters().Length == 1);

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
    /// Determines whether or not the object is a whole number
    /// </summary>
    /// <param name="t">The type we are testing</param>
    /// <returns>True if it is a whole number, else false</returns>
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
                   t == typeof(byte);
    }

    /// <summary>
    /// Determines whether or not the object is a real number
    /// </summary>
    /// <param name="t">The type we are testing</param>
    /// <returns>True if it is a real number, else false</returns>
    public static bool IsRealNumber(this Type t)
    {
        return
                   t == typeof(float) ||
                   t == typeof(double) ||
                   t == typeof(decimal);
    }

    /// <summary>
    /// Determines whether or not the object is a number
    /// </summary>
    /// <param name="t">The type we are testing</param>
    /// <returns>True if it is a number, else false</returns>
    public static bool IsNumber(this Type t)
    {
        return IsWholeNumber(t) || IsRealNumber(t);
    }

    /// <summary>
    /// Gets the max and min values for the given numeric type
    /// </summary>
    /// <param name="t">The given numeric type</param>
    /// <param name="max">The maximum possible value</param>
    /// <param name="min">The minimum possible value</param>
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
            max = double.MaxValue; //who knew!?!?!  decimals are smale
            min = double.MinValue;
        }
        else if (t == typeof(decimal))
        {
            max = Convert.ToDouble(decimal.MaxValue);
            min = Convert.ToDouble(decimal.MinValue);
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

    public static Type GetUnderlyingType(this MemberInfo mi)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        return mi is PropertyInfo pi ? pi.PropertyType : mi is FieldInfo fi ? fi.FieldType : null;
    }

    public static object GetValue(this MemberInfo mi, object basevar)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        return mi is PropertyInfo pi
            ? pi.GetValue(basevar, null)
            : mi is FieldInfo fi ? fi.GetValue(basevar) : throw new UnexpectedSwitchValueException(mi.GetType().Name);
    }

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

    public static object ConvertValue(this MemberInfo mi, object val)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        return mi is PropertyInfo pi
            ? ConvertValue(pi.PropertyType, val)
            : mi is FieldInfo fi ? ConvertValue(fi.FieldType, val) : throw new UnexpectedSwitchValueException(mi.GetType().Name);
    }

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

    public static object ConvertValue(Type t, object val)
    {
        object v = null;
        if (val == null)
        {
            Stuff.Noop();
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

    public static bool CanWrite(this MemberInfo mi)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        return mi is PropertyInfo pi ? pi.CanWrite : mi is FieldInfo fi && !(fi.IsInitOnly || fi.IsLiteral);
    }

    //        public static bool A

    public static AssemblyAttributeInfo GetInfo(this Assembly a)
    {
        return new AssemblyAttributeInfo(a);
    }

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

    public static ConstructorInfo GetConstructorNoParameters(this Type test)
        => test.GetConstructors(BindingFlags.Public).FirstOrDefault(ci => ci.GetParameters().Length == 0);

    public static PropertyInfo[] GetPropertiesPublicInstanceRead(this Type test)
        => test.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

    public static PropertyInfo[] GetPropertiesPublicInstanceReadWrite(this Type test)
        => test.GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

    public static FieldInfo[] GetFieldsPublicInstanceRead(this Type test)
        => test.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);

    public static FieldInfo[] GetFieldsPublicInstanceReadWrite(this Type test)
        => test.GetFields(BindingFlags.SetField | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);

    public static bool IsA<T>(this Type test)
        => IsA(test, typeof(T));

    public static bool IsA(this Type test, Type potentialBaseType)
    {
        return potentialBaseType.GetTypeInfo().IsAssignableFrom(test);
    }

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
