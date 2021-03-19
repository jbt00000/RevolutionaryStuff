namespace RevolutionaryStuff.Core.ApplicationParts.Services.DependencyInjection
{
    public interface INamedFactory
    {
        T GetServiceByName<T>(string name);
    }


}
