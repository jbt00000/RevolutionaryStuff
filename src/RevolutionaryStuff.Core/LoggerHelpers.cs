using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core;

public static class LoggerHelpers
{
    public static void LogError(this ILogger logger, Exception ex)
        => logger.LogError(ex, ex.Message);

    public static IDisposable CreateLogRegion(this ILogger logger, string message, params object[] scopeArgs)
        => new RevolutionaryStuff.Core.Diagnostics.LogRegion(logger, message, scopeArgs);

    public static IDisposable CreateLogRegion(this ILogger logger, LogLevel logLevel, string message, params object[] scopeArgs)
        => new RevolutionaryStuff.Core.Diagnostics.LogRegion(logger, logLevel, message, scopeArgs);

    public static IDisposable LogScopedProperty(this ILogger logger, string propertyName, object propertyValue)
        => logger.BeginScope("{propertyName}:{propertyValue}", propertyName, propertyValue);
}
