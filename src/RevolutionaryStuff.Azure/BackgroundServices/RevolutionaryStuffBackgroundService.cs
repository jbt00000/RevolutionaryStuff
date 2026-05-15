using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Azure.BackgroundServices;

public abstract class RevolutionaryStuffBackgroundService : BackgroundService
{
    protected IServiceProvider ServiceProvider { get; private set; }

    public sealed record RevolutionaryStuffBackgroundServiceConstructorArgs(IServiceProvider ServiceProvider, ILoggerFactory LoggerFactory);

    protected RevolutionaryStuffBackgroundService(RevolutionaryStuffBackgroundServiceConstructorArgs constructorArgs)
    {
        ArgumentNullException.ThrowIfNull(constructorArgs);
        ArgumentNullException.ThrowIfNull(constructorArgs.LoggerFactory);

        ServiceProvider = constructorArgs.ServiceProvider;
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
                    var logger = LoggerFactory?.CreateLogger(typeof(RevolutionaryStuffBackgroundService)) ?? new NullLogger<RevolutionaryStuffBackgroundService>();
                    return logger;
                }

            }
            return field;
        }
    }

    private readonly ILoggerFactory LoggerFactory;

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

    protected void LogError(Exception ex, [CallerMemberName] string caller = null)
        => Logger.LogError(ex, ex?.Message);

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
