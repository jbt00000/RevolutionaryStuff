namespace RevolutionaryStuff.Core.Services.DependencyInjection;

public interface INamedFactory
{
    T GetServiceByName<T>(string name);
}
