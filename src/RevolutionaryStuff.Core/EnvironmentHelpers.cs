namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides helpers for reading the application's environment name.
/// </summary>
public static class EnvironmentHelpers
{
    /// <summary>
    /// The environment variable used by default to read the environment name.
    /// </summary>
    public const string StandardEnvironmentNameVariableName = "ENV";

    /// <summary>
    /// Gets the environment name from an environment variable.
    /// </summary>
    /// <param name="environmentNameVariableName">The variable to read, or <see langword="null"/> to use <see cref="StandardEnvironmentNameVariableName"/>.</param>
    /// <returns>The variable's value, or <see langword="null"/> if the variable is not defined.</returns>
    public static string GetEnvironmentName(string environmentNameVariableName = null)
        => Environment.GetEnvironmentVariable(StringHelpers.Coalesce(environmentNameVariableName, StandardEnvironmentNameVariableName));

    /// <summary>
    /// Contains commonly used environment names.
    /// </summary>
    public static class CommonEnvironmentNames
    {
        /// <summary>
        /// The default environment name, which is <see cref="Production"/>.
        /// </summary>
        public const string Default = Production;

        /// <summary>
        /// The development environment name.
        /// </summary>
        public const string Development = "Development";

        /// <summary>
        /// The production environment name.
        /// </summary>
        public const string Production = "Production";
    }
}
