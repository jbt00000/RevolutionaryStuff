namespace RevolutionaryStuff.Data.JsonStore.Repos;

public record RepoCacheRequirements(bool AllowFetchingOfCachedResults)
{
    public static readonly RepoCacheRequirements Fresh = new(false);

    public static readonly RepoCacheRequirements Cached = new(true);

    public static readonly RepoCacheRequirements Default = Fresh;
}

