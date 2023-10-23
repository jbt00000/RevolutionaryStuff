namespace RevolutionaryStuff.Core.ApplicationParts;

public static class EnvironmentNames
{
    public static IEnumerable<string> AllNames => new[]
    {
        Production.Name,
        Staging.Name,
        Development.Name,
        Experimentation.Name,
        Infrastructure.Name
    };

    public static IEnumerable<string> AllAbbreviations => new[]
    {
        Production.InfrastructureAbbreviation,
        Staging.InfrastructureAbbreviation,
        Development.InfrastructureAbbreviation,
        Experimentation.InfrastructureAbbreviation,
        Infrastructure.InfrastructureAbbreviation
    };

    public static class Production
    {
        public const string Name = "Production";
        public const string InfrastructureAbbreviation = "prd";
    }

    public static class Staging
    {
        public const string Name = "Staging";
        public const string InfrastructureAbbreviation = "stg";
    }

    public static class Development
    {
        public const string Name = "Development";
        public const string InfrastructureAbbreviation = "dev";
    }

    public static class Experimentation
    {
        public const string Name = "Experimentation";
        public const string InfrastructureAbbreviation = "exp";
    }

    public static class Infrastructure
    {
        public const string Name = "Infrastructure";
        public const string InfrastructureAbbreviation = "inf";
    }
}
