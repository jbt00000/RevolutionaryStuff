using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.JsonStore.Store;

public class UpdateOptions : IValidate
{
    public bool ReloadBeforeUpdate { get; set; }

    public bool ForceUpdate { get; set; }

    public void Validate()
    { }
}
