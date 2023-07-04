namespace RevolutionaryStuff.Data.Cosmos.Services.Setup;
public interface ICosmosAdministration
{
    Task SetupContainerAsync(string connectionString, string databaseId, ContainerSetupInfo containerBootstrapInfo);
}
