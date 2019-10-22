using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core.Diagnostics
{
    public class LogRegion : BaseDisposable
    {
        private readonly ILogger Logger;
        private readonly IDisposable LogScope;
        private readonly Stopwatch Stopwatch;

        #region Constructors

        public LogRegion(ILogger logger, [CallerMemberName] string message = null, params object[] args)
        {
            Requires.NonNull(logger, nameof(logger));
            Logger = logger;
            LogScope = logger.BeginScope(message, args);
            Stopwatch = Stopwatch.StartNew();
        }

        #endregion

        protected override void OnDispose(bool disposing)
        {
            Stopwatch.Stop();
            Logger.LogDebug("Duration = {duration}", Stopwatch.Elapsed);
            Stuff.Dispose(LogScope);
            base.OnDispose(disposing);
        }
    }
}
