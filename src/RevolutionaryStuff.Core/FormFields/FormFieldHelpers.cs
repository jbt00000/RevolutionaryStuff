using System.Collections;
using System.Reflection;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.Core.FormFields;

public static class FormFieldHelpers
{
    public class ConversionSettings
    {
        public EnumerationSerializationOptions EnumerationSerializationOption { get; set; } = EnumerationSerializationOptions.AsEnum;

        internal static readonly ConversionSettings Default = new()
        {

        };
    }

    public enum EnumerationSerializationOptions
    {
        AsEnum,
        AsString,
        AsNumber,
    }

    [Obsolete]
    public static IEnumerable<KeyValuePair<string, object>> ConvertObjectToKeyValuePairs(object root, ConversionSettings settings = null)
    {
        settings ??= ConversionSettings.Default;
        var items = new List<KeyValuePair<string, object>>();
        var seen = new HashSet<object>();
        if (root != null)
        {
            ConvertObjectToKeyValuePairs(root, root.GetType(), null, settings, items, seen, true);
        }
        return items;
    }

    private static KeyValuePair<string, object> CreateItem(string key, object val) => new(key, val);

    [Obsolete]
    private static void ConvertObjectToKeyValuePairs(object o, Type t, MemberInfo mi, ConversionSettings settings, IList<KeyValuePair<string, object>> items, HashSet<object> seen, bool forceTreatAsContainer)
    {
        if (o != null)
        {
            if (seen.Contains(o)) return;
            var fra = mi?.GetCustomAttribute<FormFieldRepeaterAttribute>();
            if (fra != null && t.IsA(typeof(IEnumerable)))
            {
                seen.Add(o);
                var i = 0;
                foreach (var kid in (IEnumerable)o)
                {
                    var subs = new List<KeyValuePair<string, object>>();
                    ConvertObjectToKeyValuePairs(kid, kid == null ? typeof(object) : kid.GetType(), null, settings, subs, seen, true);
                    foreach (var kvp in subs)
                    {
                        items.Add(CreateItem(fra.TransformName(kvp.Key, i), kvp.Value));
                    }
                    ++i;
                }
                return;
            }
            var ffda = mi?.GetCustomAttribute<FormFieldDictionaryAttribute>();
            if (ffda != null && o is IEnumerable<KeyValuePair<string, object>>)
            {
                foreach (var kvp in (IEnumerable<KeyValuePair<string, object>>)o)
                {
                    var name = ffda.TransformName(kvp.Key);
                    items.Add(CreateItem(name, kvp.Value));
                }
            }
            else
            {
                var ffca = mi?.GetCustomAttribute<FormFieldContainerAttribute>();
                if (ffca != null || forceTreatAsContainer)
                {
                    seen.Add(o);
                    var serializableChildren = 0;
                    var subs = new List<KeyValuePair<string, object>>();
                    foreach (var pi in t.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (pi.GetCustomAttribute<FormFieldSerializable>() == null) continue;
                        ++serializableChildren;
                        var val = pi.GetValue(o);
                        var valType = pi.PropertyType;
                        ConvertObjectToKeyValuePairs(val, valType, pi, settings, subs, seen, false);
                    }
                    if (serializableChildren > 0)
                    {
                        foreach (var kvp in subs)
                        {
                            var name = ffca?.TransformName(kvp.Key) ?? kvp.Key;
                            items.Add(CreateItem(name, kvp.Value));
                        }
                        return;
                    }
                }
            }
            if (t.IsEnum)
            {
                switch (settings.EnumerationSerializationOption)
                {
                    case EnumerationSerializationOptions.AsEnum:
                        break;
                    case EnumerationSerializationOptions.AsNumber:
                        o = (int)o;
                        break;
                    case EnumerationSerializationOptions.AsString:
                        o = EnumeratedStringValueAttribute.GetValue((Enum)o) ?? o.ToString();
                        break;
                    default:
                        throw new UnexpectedSwitchValueException(settings.EnumerationSerializationOption);
                }
            }
        }
        if (mi == null)
        {
            items.Add(new KeyValuePair<string, object>("", o));
        }
        else
        {
            var fieldName = mi.Name;
            var ffa = mi.GetCustomAttribute<FormFieldAttribute>();
            if (ffa != null)
            {
                fieldName = StringHelpers.Coalesce(ffa.FieldName, fieldName);
            }
            var tffa = mi.GetCustomAttribute<TransformedFormFieldAttribute>();
            if (tffa != null)
            {
                o = tffa.Transform(o);
            }
            if (o != null)
            {
                items.Add(new KeyValuePair<string, object>(fieldName, o));
            }
        }
    }
}
