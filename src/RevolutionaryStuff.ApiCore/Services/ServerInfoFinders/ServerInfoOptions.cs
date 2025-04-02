namespace RevolutionaryStuff.ApiCore.Services.ServerInfoFinders;

public record ServerInfoOptions
{
    public static readonly ServerInfoOptions Default = new();
    public bool PopulateEnvironmentVariables { get; init; }
    public bool PopulateConfigs { get; init; }
}
