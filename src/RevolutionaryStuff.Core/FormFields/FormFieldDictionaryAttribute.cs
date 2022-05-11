namespace RevolutionaryStuff.Core.FormFields;

[AttributeUsage(AttributeTargets.Property)]
public class FormFieldDictionaryAttribute : FormFieldContainerAttribute
{
    public FormFieldDictionaryAttribute(string pattern = FieldNameToken)
        : base(pattern)
    { }
}

