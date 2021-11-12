using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Core;

public abstract class BaseLoggingDisposable : BaseDisposable
{
    protected BaseLoggingDisposable(ILogger logger)
        : base()
    {
        Requires.NonNull(logger, nameof(logger));

        Logger = logger;
    }

    #region Logging

    protected readonly ILogger Logger;

    protected void LogWarning(string message, params object[] args)
        => Logger.LogWarning(message, args);

    protected void LogInformation(string message, params object[] args)
        => Logger.LogInformation(message, args);

    protected void LogError(string message, params object[] args)
        => Logger.LogError(message, args);

    protected void LogError(Exception ex, string message, params object[] args)
        => Logger.LogError(ex, message, args);

    protected void LogException(Exception ex, [CallerMemberName] string caller = null)
        => Logger.LogError(ex, "Invoked from {caller}", caller);

    protected void LogDebug(string message, params object[] args)
        => Logger.LogDebug(message, args);

    protected void LogTrace(string message, params object[] args)
        => Logger.LogTrace(message, args);

    protected IDisposable CreateLogRegion([CallerMemberName] string message = null, params object[] args)
        => new LogRegion(Logger, message, args);

    #endregion

    protected async Task ActAsync(Func<Task> executeAsync, [CallerMemberName] string caller = null)
    {
        try
        {
            LogInformation("{caller} function started processing", caller);
            Requires.NonNull(executeAsync, nameof(executeAsync));
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
