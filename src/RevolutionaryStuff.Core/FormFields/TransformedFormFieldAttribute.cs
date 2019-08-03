using System;

namespace RevolutionaryStuff.Core.FormFields
{
    public abstract class TransformedFormFieldAttribute : Attribute
    {
        public abstract object Transform(object val);
    }
}
