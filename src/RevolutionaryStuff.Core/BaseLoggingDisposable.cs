using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Core;

public abstract class BaseLoggingDisposable : BaseDisposable
{
    protected BaseLoggingDisposable(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        Logger = logger;
    }

    #region Logging

    protected readonly ILogger Logger;

    protected void LogTrace(string message, params object[] args)
        => Logger.LogTrace(message, args);

    protected void LogWarning(string message, params object[] args)
        => Logger.LogWarning(message, args);

    protected void LogInformation(string message, params object[] args)
        => Logger.LogInformation(message, args);

    protected void LogError(string message, params object[] args)
        => Logger.LogError(message, args);

    protected void LogError(Exception ex, string message, params object[] args)
    {
        using var _ = Logger.BeginScope("{ex}", ex);
        Logger.LogError(message, args);
    }

    protected void LogException(Exception ex, [CallerMemberName] string caller = null)
        => Logger.LogError(ex, "Invoked from {caller}", caller);

    protected void LogCritical(string message, params object[] args)
        => Logger.LogCritical(message, args);

    protected void LogCritical(Exception ex, string message, params object[] args)
        => Logger.LogCritical(ex, message, args);

    protected void LogCritical(EventId eventId, Exception ex, string message, params object[] args)
        => Logger.LogCritical(eventId, ex, message, args);

    protected void LogDebug(string message, params object[] args)
        => Logger.LogDebug(message, args);

    protected IDisposable CreateLogRegion(LogLevel level, [CallerMemberName] string message = null, params object[] args)
        => new LogRegion(Logger, level, message, args);

    protected IDisposable CreateLogRegion([CallerMemberName] string message = null, params object[] args)
        => new LogRegion(Logger, message, args);

    protected IDisposable LogScopedProperty(string propertyName, object propertyValue, bool decomposeValue=false)
        => Logger.LogScopedProperty(propertyName, propertyValue, decomposeValue);

    #endregion

    protected async Task ActAsync(Func<Task> executeAsync, [CallerMemberName] string caller = null)
    {
        try
        {
            LogInformation("{caller} function started processing", caller);
            ArgumentNullException.ThrowIfNull(executeAsync);
            await executeAsync();
            LogInformation("{caller} function completed", caller);
        }
        catch (Exception ex)
        {
            LogException(ex);
            throw;
        }
    }
}
