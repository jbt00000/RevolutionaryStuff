using System.Reflection;

namespace RevolutionaryStuff.Core.Services.ApplicationNameFinders;

public class AssemblyApplicationNameFinder : HardcodedApplicationNameFinder
{
    public AssemblyApplicationNameFinder(Assembly a)
        : base(a.GetType().GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? a.GetName().Name)
    { }
}
