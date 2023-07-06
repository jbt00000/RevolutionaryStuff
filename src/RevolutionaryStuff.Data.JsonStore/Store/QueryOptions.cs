using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.JsonStore.Store;

public class QueryOptions : IValidate
{
    public string? PartitionKey { get; set; }

    public bool IgnoreEntityDataType { get; set; }

    public void Validate()
    { }

    public static QueryOptions CreateWithParitionKey(string partitionKey)
        => new()
        {
            PartitionKey = partitionKey
        };
}
