using System;

namespace RevolutionaryStuff.Core.ApplicationParts.Services.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NamedTypeAttribute : Attribute
    {
        public readonly string[] Names;

        public NamedTypeAttribute(params string[] names)
        {
            Names = names;
        }
    }


}
