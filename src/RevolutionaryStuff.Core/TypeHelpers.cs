using System;
using System.Collections.Generic;
using System.Reflection;

namespace RevolutionaryStuff.Core
{
    public static class TypeHelpers
    {
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

        private static void RequiresIsPropertyInfoOrFieldInfo(MemberInfo mi)
        {
            Requires.NonNull(mi, nameof(mi));
            if (mi is PropertyInfo || mi is FieldInfo) return;
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
                return Convert.ChangeType(val, t);
            }
            catch (InvalidCastException)
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
                throw new NotSupportedException();
            }
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
                string sval = StringHelpers.ToString(val);
                bool b;
                if (Parse.TryParseBool(sval, out b)) return b;
                throw new NotSupportedException(string.Format("ChangeType could change [{0}] into a bool.", val));
            }
            else if (t.GetTypeInfo().IsEnum)
            {
                return Enum.Parse(t, val.ToString(), true);
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
            int depth=0, 
            HashSet<Type> loopChecker = null,
            MemberInfo baseTypeMemberInfo = null)
        {
            Requires.NonNull(baseType, nameof(baseType));
            Requires.NonNull(visit, nameof(visit));

            recurse = recurse ?? delegate (TNodeContext a, Type b, MemberInfo c) { return true; };
            var nodeContext = createContext(parentNodeContext, baseType, baseTypeMemberInfo);

            loopChecker = loopChecker ?? new HashSet<Type>();
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
}
