using System.Collections;
using System.Dynamic;
using System.Reflection;

namespace RevolutionaryStuff.Core;

public static class TypeHelpers
{
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
        if (t.IsValueType)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        return true;
    }

    public static object GetDefaultValue(this Type t)
    {
        if (t.GetTypeInfo().IsValueType)
        {
            return Activator.CreateInstance(t);
        }
        return null;
    }

    public static object Construct(this Type t)
    {
        if (t.Name == "IList`1")
        {
            return ConstructList(t.GenericTypeArguments[0]);
        }
        else if (t.Name == "IDictionary`2")
        {
            return ConstructDictionary(t.GenericTypeArguments[0], t.GenericTypeArguments[1]);
        }
        else
        {
            return Activator.CreateInstance(t);
        }
    }

    public static T Construct<T>() where T : new()
        => (T)Construct(typeof(T));

    public static IDictionary ConstructDictionary(Type keyType, Type valType)
    {
        var gt = typeof(Dictionary<,>).MakeGenericType(new[] { keyType, valType });
        return (IDictionary)Construct(gt);
    }

    public static IList ConstructList(Type itemType)
    {
        var gt = typeof(List<>).MakeGenericType(new[] { itemType });
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
            if (d != null) return d.AsReadOnly();
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
        return (
                   t == typeof(Int16) ||
                   t == typeof(Int32) ||
                   t == typeof(Int64) ||
                   t == typeof(UInt16) ||
                   t == typeof(UInt32) ||
                   t == typeof(UInt64) ||
                   t == typeof(SByte) ||
                   t == typeof(Byte));
    }

    /// <summary>
    /// Determines whether or not the object is a real number
    /// </summary>
    /// <param name="t">The type we are testing</param>
    /// <returns>True if it is a real number, else false</returns>
    public static bool IsRealNumber(this Type t)
    {
        return (
                   t == typeof(Single) ||
                   t == typeof(Double) ||
                   t == typeof(Decimal));
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
        if (t == typeof(Int16))
        {
            max = Int16.MaxValue;
            min = Int16.MinValue;
        }
        else if (t == typeof(Int32))
        {
            max = Int32.MaxValue;
            min = Int32.MinValue;
        }
        else if (t == typeof(Int64))
        {
            max = Int64.MaxValue;
            min = Int64.MinValue;
        }
        else if (t == typeof(UInt16))
        {
            max = UInt16.MaxValue;
            min = UInt16.MinValue;
        }
        else if (t == typeof(UInt32))
        {
            max = UInt32.MaxValue;
            min = UInt32.MinValue;
        }
        else if (t == typeof(UInt64))
        {
            max = UInt64.MaxValue;
            min = UInt64.MinValue;
        }
        else if (t == typeof(SByte))
        {
            max = SByte.MaxValue;
            min = SByte.MinValue;
        }
        else if (t == typeof(Byte))
        {
            max = Byte.MaxValue;
            min = Byte.MinValue;
        }
        else if (t == typeof(Single))
        {
            max = Single.MaxValue;
            min = Single.MinValue;
        }
        else if (t == typeof(Double))
        {
            max = Double.MaxValue; //who knew!?!?!  decimals are smale
            min = Double.MinValue;
        }
        else if (t == typeof(Decimal))
        {
            max = Convert.ToDouble(Decimal.MaxValue);
            min = Convert.ToDouble(Decimal.MinValue);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(t), $"{t?.Name} was not numeric");
        }
    }

    private static void RequiresIsPropertyInfoOrFieldInfo(MemberInfo mi)
    {
        Requires.NonNull(mi, nameof(mi));
        if (mi is PropertyInfo or FieldInfo) return;
        throw new Exception(string.Format("we weren't expectecting a {0}", mi.GetType()));
    }

    public static Type GetUnderlyingType(this MemberInfo mi)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        if (mi is PropertyInfo)
        {
            var pi = (PropertyInfo)mi;
            return pi.PropertyType;
        }
        else if (mi is FieldInfo)
        {
            var fi = (FieldInfo)mi;
            return fi.FieldType;
        }
        return null;
    }

    public static object GetValue(this MemberInfo mi, object basevar)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        if (mi is PropertyInfo)
        {
            var pi = (PropertyInfo)mi;
            return pi.GetValue(basevar, null);
        }
        else if (mi is FieldInfo)
        {
            var fi = (FieldInfo)mi;
            return fi.GetValue(basevar);
        }
        throw new UnexpectedSwitchValueException(mi.GetType().Name);
    }

    public static void SetValue(this MemberInfo mi, object basevar, object val)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        object v = mi.ConvertValue(val);
        if (mi is PropertyInfo)
        {
            var pi = (PropertyInfo)mi;
            pi.SetValue(basevar, v, null);
        }
        else if (mi is FieldInfo)
        {
            var fi = (FieldInfo)mi;
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
        if (mi is PropertyInfo)
        {
            var pi = (PropertyInfo)mi;
            return ConvertValue(pi.PropertyType, val);
        }
        else if (mi is FieldInfo)
        {
            var fi = (FieldInfo)mi;
            return ConvertValue(fi.FieldType, val);
        }
        throw new UnexpectedSwitchValueException(mi.GetType().Name);
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
            else if (t == typeof(bool))
            {
                return Parse.ParseBool(val);
            }
            else if (t == typeof(Uri))
            {
                return new Uri(val);
            }
            else if (ti.IsEnum)
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
            if (ti.IsAssignableFrom(val.GetType()))
            {
                v = val;
            }
            else if (ti.IsEnum)
            {
                v = Enum.Parse(t, val.ToString(), true);
            }
            else if (
                ti.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                t.GenericTypeArguments.Length == 1 &&
                t.GenericTypeArguments[0].GetTypeInfo().IsEnum)
            {
                v = Enum.Parse(t.GenericTypeArguments[0], val.ToString(), true);
            }
            else
            {
                v = ChangeType(val, t);
            }
        }
        return v;
    }

    private static object ChangeType(object val, Type t)
    {
        if (t.FullName == "System.Object") return val;
        if (t == typeof(bool))
        {
            var sval = StringHelpers.ToString(val);
            bool b;
            if (Parse.TryParseBool(sval, out b)) return b;
            throw new NotSupportedException(string.Format("ChangeType could change [{0}] into a bool.", val));
        }
        else if (t.GetTypeInfo().IsEnum)
        {
            return Enum.Parse(t, val.ToString(), true);
        }
        else if (t == typeof(Uri) && val is string)
        {
            return new Uri((string)val);
        }
        else
        {
            return Convert.ChangeType(val, t, null);
        }
    }

    public static bool CanWrite(this MemberInfo mi)
    {
        RequiresIsPropertyInfoOrFieldInfo(mi);
        if (mi is PropertyInfo)
        {
            var pi = (PropertyInfo)mi;
            return pi.CanWrite;
        }
        else if (mi is FieldInfo)
        {
            var fi = (FieldInfo)mi;
            return !(fi.IsInitOnly || fi.IsLiteral);
        }
        else
        {
            return false;
        }
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
        return new List<Assembly>(d.Values);
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
        TNodeContext parentNodeContext = default(TNodeContext),
        int depth = 0,
        HashSet<Type> loopChecker = null,
        MemberInfo baseTypeMemberInfo = null)
    {
        Requires.NonNull(baseType, nameof(baseType));
        Requires.NonNull(visit, nameof(visit));

        recurse ??= delegate (TNodeContext a, Type b, MemberInfo c) { return true; };
        var nodeContext = createContext(parentNodeContext, baseType, baseTypeMemberInfo);

        loopChecker ??= new HashSet<Type>();
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
