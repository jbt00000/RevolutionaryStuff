using System;

namespace RevolutionaryStuff.Core.FormFields
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FormFieldContainerAttribute : FormFieldSerializable
    {
        public const string FieldNameToken = "{N}";

        public readonly string Pattern;

        public FormFieldContainerAttribute(string pattern)
        {
            Pattern = pattern.Contains(FieldNameToken) ? pattern : pattern + FieldNameToken;
        }

        public string TransformName(string name)
            => Pattern.Replace(FieldNameToken, name);
    }

}
