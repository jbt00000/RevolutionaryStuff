namespace RevolutionaryStuff.Data.JsonStore.Entities;

public sealed class JsonEntityIdPartitionKeyAttribute : JsonEntityPartitionKeyBaseAttribute
{
    public JsonEntityIdPartitionKeyAttribute()
        : base(PartitionKeySchemeEnum.SameAsObjectId, null)
    { }
}
