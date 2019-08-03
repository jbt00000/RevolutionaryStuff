using System;

namespace RevolutionaryStuff.Core.FormFields
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FormFieldAttribute : FormFieldSerializable
    {
        public readonly string FieldName;
        internal readonly string Prefix;
        internal readonly string Name;

        public FormFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public FormFieldAttribute(string prefix, string name)
            : this(prefix+name)
        {
            Name = name;
        }
    }
}
