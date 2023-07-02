namespace RevolutionaryStuff.Data.JsonStore.Entities;

public sealed class JsonEntityRelatedPartitionKeyAttribute<TEntity> : JsonEntityPartitionKeyBaseAttribute
    where TEntity : JsonEntity
{
    public JsonEntityRelatedPartitionKeyAttribute()
        : base(PartitionKeySchemeEnum.RelatedObjectId, typeof(TEntity))
    { }
}
