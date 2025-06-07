namespace RevolutionaryStuff.Storage;

public interface IUserPropertyStore
{
    Task<IDictionary<string, string>> GetUserPropertiesAsync();
    Task SetUserPropertiesAsync(IDictionary<string, string> properties);
}
