using System.Collections.Concurrent;
using System.Reflection;

namespace RevolutionaryStuff.Data.JsonStore.Entities;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class JsonEntityPrefixAttribute : Attribute
{
    public string Prefix;

    public const string Separator = ".";

    public bool Absolute;

    public JsonEntityPrefixAttribute(params string[] prefixParts)
    {
        Prefix = prefixParts.Join(Separator);
    }

    private static readonly IDictionary<Type, string> PrefixByType = new ConcurrentDictionary<Type, string>();

    internal static string GetPrefix(Type t)
        => PrefixByType.FindOrCreate(t, () =>
        {
            string prefix = null;
            while (t != null)
            {
                var prefixAttr = t.GetCustomAttribute<JsonEntityPrefixAttribute>(false);
                if (prefixAttr != null)
                {
                    if (prefixAttr.Absolute)
                    {
                        prefix = prefixAttr.Prefix;
                        break;
                    }
                    else
                    {
                        prefix = prefix == null ? prefixAttr.Prefix : prefixAttr.Prefix + Separator + prefix;
                    }
                }
                t = t.BaseType;
            }
            return prefix;
        });
}
