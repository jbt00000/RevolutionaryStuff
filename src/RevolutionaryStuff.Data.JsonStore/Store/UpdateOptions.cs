using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.JsonStore.Store;

public class UpdateOptions : IValidate
{
    public string? PartitionKey { get; set; }

    public string? IfMatchEtag { get; set; }

    public bool EnableContentResponseOnWrite { get; set; }

    public void Validate()
    { }
}
