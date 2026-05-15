using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.ApiCore.Razor;

public abstract class RevolutionaryStuffPageModel : PageModel
{
    public sealed record RevolutionaryStuffPageModelConstructorArgs(ILoggerFactory LoggerFactory);

    protected RevolutionaryStuffPageModel(RevolutionaryStuffPageModelConstructorArgs constructorArgs)
    {
        ArgumentNullException.ThrowIfNull(constructorArgs);
        ArgumentNullException.ThrowIfNull(constructorArgs.LoggerFactory);

        LoggerFactory = constructorArgs.LoggerFactory;
    }

    #region Logging

    protected ILogger Logger
    {
        get
        {
            if (field == null)
            {
                try
                {
                    var logger = LoggerFactory?.CreateLogger(GetType());
                    field = logger;
                }
                catch (Exception)
                { }
                if (field == null)
                {
                    var logger = LoggerFactory?.CreateLogger(typeof(RevolutionaryStuffPageModel)) ?? new NullLogger<RevolutionaryStuffPageModel>();
                    return logger;
                }
            }
            return field;
        }
    }

    private readonly ILoggerFactory LoggerFactory;

    protected void Log(LogLevel level, string message, params object[] args)
        => Logger.Log(level, message, args);

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

    protected IDisposable LogScopedProperty(string propertyName, object propertyValue, bool decomposeValue = false)
        => Logger.LogScopedProperty(propertyName, propertyValue, decomposeValue);

    #endregion
}
