namespace RevolutionaryStuff.Core.ApplicationParts;

public class ServiceProviderConnectionStringProvider : IConnectionStringProvider
{
    private readonly IServiceProvider ServiceProvider;

    public ServiceProviderConnectionStringProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    string IConnectionStringProvider.GetConnectionString(string connectionStringName)
        => ServiceProvider.GetConnectionString(connectionStringName);
}
