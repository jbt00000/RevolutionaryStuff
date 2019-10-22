using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Mergers
{
    public interface IMergeInputs : IValidate
    {
        TemplateInfo Template { get; }

        MergeDataInfo Data { get; }

        MergeOutputSettings OutputSettings { get; }
    }
}
