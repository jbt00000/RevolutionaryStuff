using System.Collections.Concurrent;

namespace RevolutionaryStuff.Data.JsonStore.Entities;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class JsonEntityRelatedKeyBaseAttribute : Attribute
{
    public RelationEnum Relation;
    public Type RelatedType;
    public string Comment;

    public enum RelationEnum
    {
        Unspecified,
        ForeignKey,
    }

    protected JsonEntityRelatedKeyBaseAttribute(RelationEnum relation, Type relatedType)
    {
        Relation = relation;
        RelatedType = relatedType;
    }

    private static readonly IDictionary<Type, RelationEnum> AttrByType = new ConcurrentDictionary<Type, RelationEnum>();

    public static RelationEnum GetScheme(Type t)
        => AttrByType.FindOrCreate(
            t,
            () =>
            {
                while (t != null)
                {
                    var attr = t.GetCustomAttribute<JsonEntityRelatedKeyBaseAttribute>(false);
                    if (attr != null)
                    {
                        return attr.Relation;
                    }
                    t = t.BaseType;
                }
                return RelationEnum.Unspecified;
            }
            );
}
