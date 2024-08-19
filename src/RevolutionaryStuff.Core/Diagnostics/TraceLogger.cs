using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core.Diagnostics;

public class TraceLogger : ILogger
{
    private readonly string CategoryName;

    public TraceLogger(string categoryName)
    {
        CategoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => default;

    public bool IsEnabled(LogLevel logLevel)
    {
        // Adjust this method as needed to filter log levels
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = $"{logLevel}: {CategoryName} - {formatter(state, exception)}";
        if (exception != null)
        {
            message += Environment.NewLine + exception;
        }

        // Write the log message to System.Diagnostics.Trace
        System.Diagnostics.Trace.WriteLine(message, logLevel.ToString());
    }
}
