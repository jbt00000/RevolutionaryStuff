using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.ApiCore.Services.HostedServices;

public abstract class BaseBackgroundService : BackgroundService
{
    protected readonly IServiceProvider ServiceProvider;

    public sealed record BaseBackgroundServiceConstructorArgs(IServiceProvider ServiceProvider)
    { }

    protected BaseBackgroundService(BaseBackgroundServiceConstructorArgs constructorArgs, ILogger logger)
    {
        ServiceProvider = constructorArgs.ServiceProvider;
        Logger = logger;
    }

    #region Logging

    private readonly ILogger Logger;

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

    protected void LogError(Exception ex, [CallerMemberName] string? caller = null)
        => Logger.LogError(ex, ex?.Message);

    protected void LogCritical(string message, params object[] args)
        => Logger.LogCritical(message, args);

    protected void LogCritical(Exception ex, string message, params object[] args)
        => Logger.LogCritical(ex, message, args);

    protected void LogCritical(EventId eventId, Exception ex, string message, params object[] args)
        => Logger.LogCritical(eventId, ex, message, args);

    protected void LogDebug(string message, params object[] args)
        => Logger.LogDebug(message, args);

    protected IDisposable CreateLogRegion(LogLevel level, [CallerMemberName] string? message = null, params object[] args)
        => new LogRegion(Logger, level, message, args);

    protected IDisposable CreateLogRegion([CallerMemberName] string? message = null, params object[] args)
        => new LogRegion(Logger, message, args);

    protected IDisposable LogScopedProperty(string propertyName, object propertyValue, bool decomposeValue = false)
        => Logger.LogScopedProperty(propertyName, propertyValue, decomposeValue);

    #endregion
}
