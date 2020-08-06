using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core
{
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

        #endregion
    }
}
