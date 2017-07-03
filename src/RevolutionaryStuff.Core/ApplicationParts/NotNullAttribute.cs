using System;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Parameter)]
    public class NotNullAttribute : Attribute
    {
    }
}
