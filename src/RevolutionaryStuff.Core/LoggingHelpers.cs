using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core;

public static class LoggingHelpers
{
    public static void LogError(this ILogger logger, Exception ex)
        => logger.LogError(ex, ex.Message);

}
