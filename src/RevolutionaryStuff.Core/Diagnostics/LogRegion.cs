using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core.Diagnostics
{
    public class LogRegion : BaseDisposable
    {
        public static string DefaultOpeningPrefix = "vvvvvvvvvvvvvvvvvvvv ";
        public static string DefaultClosingPrefix = "^^^^^^^^^^^^^^^^^^^^ ";
        public static string DefaultClosingSuffix = " Duration={duration}";
        public static readonly LogLevel DefaultLogLevel = LogLevel.Debug;

        private readonly ILogger Logger;
        private readonly string Message;
        private readonly IDisposable LogScope;
        private readonly Stopwatch Stopwatch;
        private LogLevel LogLevel;

        #region Constructors

        public LogRegion(ILogger logger, [CallerMemberName] string message = null, params object[] scopeArgs)
            : this(logger, null, message, scopeArgs)
        { }

        public LogRegion(ILogger logger, LogLevel? logLevel, [CallerMemberName] string message = null, params object[] scopeArgs)
        {
            Requires.NonNull(logger, nameof(logger));

            LogLevel = logLevel.GetValueOrDefault(DefaultLogLevel);
            Logger = logger;
            Message = message;
            LogScope = logger.BeginScope(message, scopeArgs);
            Logger.Log(LogLevel, DefaultOpeningPrefix + Message);
            Stopwatch = Stopwatch.StartNew();
        }

        #endregion

        protected override void OnDispose(bool disposing)
        {
            Stopwatch.Stop();
            Logger.Log(LogLevel, DefaultClosingPrefix + Message + DefaultClosingSuffix, Stopwatch.Elapsed);
            Stuff.Dispose(LogScope);
            base.OnDispose(disposing);
        }
    }
}
