using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core.Diagnostics;

public class TraceLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TraceLogger(categoryName);
    }

    public void Dispose()
    {
        // Implement IDisposable if needed
    }
}
