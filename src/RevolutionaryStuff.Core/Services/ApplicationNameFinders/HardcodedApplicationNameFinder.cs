namespace RevolutionaryStuff.Core.Services.ApplicationNameFinders;

public class HardcodedApplicationNameFinder(string applicationName) : IApplicationNameFinder
{
    string IApplicationNameFinder.ApplicationName => applicationName;
}
