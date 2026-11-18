using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core;

public static class LoggerHelpers
{
    extension(ILogger source)
    {
        public void LogError(Exception ex)
                => source.LogError(ex, ex?.Message);
        public IDisposable CreateLogRegion(string message, params object[] scopeArgs)
                => new RevolutionaryStuff.Core.Diagnostics.LogRegion(source, message, scopeArgs);
        public IDisposable CreateLogRegion(LogLevel logLevel, string message, params object[] scopeArgs)
                => new RevolutionaryStuff.Core.Diagnostics.LogRegion(source, logLevel, message, scopeArgs);
        public IDisposable LogScopedProperty(string propertyName, object propertyValue, bool decomposeValue = false)
                => decomposeValue ?
                    source.BeginScope($"{{@{propertyName}}}", propertyValue) :
                    source.BeginScope("{propertyName}:{propertyValue}", propertyName, propertyValue);
    }






}
