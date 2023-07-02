using System.Collections.Concurrent;
using System.Reflection;

namespace RevolutionaryStuff.Data.JsonStore.Entities;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public abstract class JsonEntityPartitionKeyBaseAttribute : Attribute
{
    public PartitionKeySchemeEnum PartitionKeyScheme;
    public Type? RelatedType;
    public string Comment;

    public enum PartitionKeySchemeEnum
    {
        Unspecified,
        SameAsObjectId,
        RelatedObjectId,
    }

    protected JsonEntityPartitionKeyBaseAttribute(PartitionKeySchemeEnum partitionKeyScheme, Type? relatedType)
    {
        PartitionKeyScheme = partitionKeyScheme;
        RelatedType = relatedType;
    }

    private static readonly IDictionary<Type, PartitionKeySchemeEnum> AttrByType = new ConcurrentDictionary<Type, PartitionKeySchemeEnum>();

    public static PartitionKeySchemeEnum GetScheme(Type t)
        => AttrByType.FindOrCreate(
            t,
            () =>
            {
                while (t != null)
                {
                    var attr = t.GetCustomAttribute<JsonEntityPartitionKeyBaseAttribute>(false);
                    if (attr != null)
                    {
                        return attr.PartitionKeyScheme;
                    }
                    t = t.BaseType;
                }
                return PartitionKeySchemeEnum.Unspecified;
            }
            );
}
