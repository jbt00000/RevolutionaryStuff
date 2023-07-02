namespace RevolutionaryStuff.Data.JsonStore.Entities;

public sealed class ForeignKeyAttribute<TEntity> : JsonEntityRelatedKeyBaseAttribute
    where TEntity : JsonEntity
{
    public ForeignKeyAttribute()
        : base(RelationEnum.ForeignKey, typeof(TEntity))
    { }
}
