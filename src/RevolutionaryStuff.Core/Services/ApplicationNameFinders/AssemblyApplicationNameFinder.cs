using System.Reflection;

namespace RevolutionaryStuff.Core.Services.ApplicationNameFinders;

public class AssemblyApplicationNameFinder : HardcodedApplicationNameFinder, IAssemblyApplicationNameFinder
{
    public AssemblyApplicationNameFinder()
        : this(Assembly.GetEntryAssembly())
    { }

    public AssemblyApplicationNameFinder(Assembly a)
        : base(a.GetType().GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? a.GetName().Name)
    { }
}

public interface IAssemblyApplicationNameFinder : IApplicationNameFinder
{ }
