namespace RevolutionaryStuff.Core.FormFields;

[AttributeUsage(AttributeTargets.Property)]
public class FormFieldRepeaterAttribute : FormFieldContainerAttribute
{
    public const string IndexToken = "{I}";

    public readonly int IndexBasis = 0;

    public FormFieldRepeaterAttribute(string pattern, int indexBasis = 0)
        : base(pattern.Contains(IndexToken) ? pattern : pattern + IndexToken)
    {
        IndexBasis = indexBasis;
    }

    public string TransformName(string name, int index)
        => base.TransformName(name).Replace(IndexToken, (IndexBasis + index).ToString());
}
