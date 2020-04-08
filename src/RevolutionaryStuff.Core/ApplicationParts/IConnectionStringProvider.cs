namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface IConnectionStringProvider
    {
        string GetConnectionString(string connectionStringName);
    }
}
