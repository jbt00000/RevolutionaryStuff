namespace RevolutionaryStuff.Core.Services.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
public sealed class NamedServiceAttribute : Attribute
{
    public readonly string[] ServiceNames;

    public NamedServiceAttribute(params string[] serviceNames)
    {
        ServiceNames = serviceNames;
    }
}
