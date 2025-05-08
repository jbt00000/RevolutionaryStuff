namespace RevolutionaryStuff.Data.JsonStore.Repos;

public class JsonEntityRepoBaseConfig
{
    public const string ConfigSectionName = "JsonEntityRepoBaseConfig";

    public TimeSpan CacheTimeout { get; set; }
}
