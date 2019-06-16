using System;

namespace RevolutionaryStuff.Core.FormFields
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FormFieldDictionaryAttribute : FormFieldContainerAttribute
    {
        public FormFieldDictionaryAttribute(string pattern = FormFieldContainerAttribute.FieldNameToken)
            : base(pattern)
        { }
    }

}
