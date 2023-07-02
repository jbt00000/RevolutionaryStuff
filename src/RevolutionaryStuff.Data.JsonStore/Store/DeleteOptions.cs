using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.JsonStore.Store;

public class DeleteOptions : IValidate
{
    public bool ForceHardDelete { get; set; }

    public void Validate()
    { }
}
