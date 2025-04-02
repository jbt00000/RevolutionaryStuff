using Microsoft.Extensions.Hosting;

namespace RevolutionaryStuff.Core.Services.ApplicationNameFinders;

public class HostEnvironmentApplicationNameFinder(IHostEnvironment HostEnvironment) : IApplicationNameFinder, IHostEnvironmentApplicationNameFinder
{
    string IApplicationNameFinder.ApplicationName
        => HostEnvironment.ApplicationName;
}

public interface IHostEnvironmentApplicationNameFinder : IApplicationNameFinder
{ }
