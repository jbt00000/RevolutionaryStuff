namespace RevolutionaryStuff.Data.JsonStore.Entities;

public sealed class JsonEntityForeignKeyAttribute<TEntity> : JsonEntityRelatedKeyBaseAttribute
    where TEntity : JsonEntity
{
    public JsonEntityForeignKeyAttribute()
        : base(RelationEnum.ForeignKey, typeof(TEntity))
    { }
}
