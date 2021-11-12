namespace RevolutionaryStuff.Core;

public static class EnvironmentHelpers
{
    public const string StandardEnvironmentNameVariableName = "ENV";

    public static string GetEnvironmentName(string environmentNameVariableName = null)
        => Environment.GetEnvironmentVariable(StringHelpers.Coalesce(environmentNameVariableName, StandardEnvironmentNameVariableName));

    public static class CommonEnvironmentNames
    {
        public const string Default = Production;
        public const string Development = "Development";
        public const string Production = "Production";
    }
}
