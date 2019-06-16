using System;

namespace RevolutionaryStuff.Core.FormFields
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FormFieldAttribute : FormFieldSerializable
    {
        public readonly string FieldName;

        public FormFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
