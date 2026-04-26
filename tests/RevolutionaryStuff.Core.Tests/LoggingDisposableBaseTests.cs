using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class LoggingDisposableBaseTests
{
    #region Test helpers

    private sealed class LogEntry
    {
        public LogLevel Level { get; init; }
        public string Message { get; init; }
    }

    /// <summary>Minimal ILogger that records every message written to it.</summary>
    private sealed class RecordingLogger : ILogger
    {
        public IReadOnlyList<LogEntry> Entries => EntryList;
        private readonly List<LogEntry> EntryList = [];

        public IDisposable BeginScope<TState>(TState state) => NullLogger.Instance.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => EntryList.Add(new LogEntry { Level = logLevel, Message = formatter(state, exception) });
    }

    /// <summary>Minimal ILoggerFactory backed by a single RecordingLogger.</summary>
    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        public readonly RecordingLogger Logger = new();
        public ILogger CreateLogger(string categoryName) => Logger;
        public void AddProvider(ILoggerProvider provider) { }
        public void Dispose() { }
    }

    // --- Concrete subclasses used by tests ---

    /// <summary>Logs one message during its own constructor — exercises the ILogger constructor path.</summary>
    private sealed class LogsInCtorViaILogger : LoggingDisposableBase
    {
        public LogsInCtorViaILogger(ILogger logger) : base(logger)
            => LogInformation("ctor-via-ilogger");
    }

    /// <summary>Logs one message during its own constructor — exercises the ILoggerFactory path.</summary>
    private class LogsInCtorViaFactory : LoggingDisposableBase
    {
        public LogsInCtorViaFactory(ILoggerFactory factory) : base(factory)
            => LogInformation("ctor-via-factory");
    }

    /// <summary>Exposes Logger publicly so tests can assert identity / nullness.</summary>
    private sealed class ExposedLogger : LoggingDisposableBase
    {
        public ILogger PublicLogger => Logger;

        public ExposedLogger(ILogger logger) : base(logger) { }
        public ExposedLogger(ILoggerFactory factory) : base(factory) { }
    }

    /// <summary>Calls every log-level helper once.</summary>
    private sealed class AllLevelLogger : LoggingDisposableBase
    {
        public AllLevelLogger(ILogger logger) : base(logger) { }

        public void LogAll()
        {
            LogTrace("trace");
            LogDebug("debug");
            LogInformation("info");
            LogWarning("warning");
            LogError("error");
            LogCritical("critical");
        }
    }

    /// <summary>Derived class that also logs in its constructor, testing two-level inheritance.</summary>
    private sealed class DerivedLogsInCtor : LogsInCtorViaFactory
    {
        public DerivedLogsInCtor(ILoggerFactory factory) : base(factory)
            => LogInformation("derived-ctor");
    }


    #endregion

    #region ILogger constructor path

    [TestMethod]
    public void ILoggerConstructor_LoggerPropertyIsNotNull()
    {
        var recording = new RecordingLogger();
        var sut = new ExposedLogger(recording);
        Assert.IsNotNull(sut.PublicLogger);
    }

    [TestMethod]
    public void ILoggerConstructor_LoggerPropertyReturnsSameInstance()
    {
        var recording = new RecordingLogger();
        var sut = new ExposedLogger(recording);
        Assert.AreSame(sut.PublicLogger, sut.PublicLogger);
    }

    [TestMethod]
    public void ILoggerConstructor_LoggingFromConstructorIsRecorded()
    {
        var recording = new RecordingLogger();
        _ = new LogsInCtorViaILogger(recording);
        Assert.AreEqual(1, recording.Entries.Count);
        Assert.AreEqual(LogLevel.Information, recording.Entries[0].Level);
    }

    [TestMethod]
    public void ILoggerConstructor_RequiresNonNullLogger()
        => Assert.ThrowsExactly<ArgumentNullException>(() => new ExposedLogger((ILogger)null));

    #endregion

    #region ILoggerFactory constructor path

    [TestMethod]
    public void ILoggerFactoryConstructor_LoggerPropertyIsNotNull()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ExposedLogger(factory);
        Assert.IsNotNull(sut.PublicLogger);
    }

    [TestMethod]
    public void ILoggerFactoryConstructor_LoggerPropertyReturnsSameInstanceOnRepeatedAccess()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ExposedLogger(factory);
        var first = sut.PublicLogger;
        var second = sut.PublicLogger;
        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void ILoggerFactoryConstructor_LoggingFromConstructorIsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        _ = new LogsInCtorViaFactory(factory);
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Information, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void ILoggerFactoryConstructor_RequiresNonNullFactory()
        => Assert.ThrowsExactly<ArgumentNullException>(() => new ExposedLogger((ILoggerFactory)null));

    [TestMethod]
    public void ILoggerFactoryConstructor_NullLoggerFactory_LoggerIsNullLoggerFallback()
    {
        // NullLoggerFactory.Instance is a valid factory that never throws; Logger should still be non-null.
        var sut = new ExposedLogger(NullLoggerFactory.Instance);
        Assert.IsNotNull(sut.PublicLogger);
    }

    #endregion

    #region Derived-constructor logging

    [TestMethod]
    public void LoggingFromDerivedConstructor_ILogger_DoesNotThrow()
    {
        var recording = new RecordingLogger();
        _ = new LogsInCtorViaILogger(recording);
        // No exception is the assertion; also verify the message arrived.
        Assert.AreEqual(1, recording.Entries.Count);
    }

    [TestMethod]
    public void LoggingFromDerivedConstructor_ILoggerFactory_DoesNotThrow()
    {
        var factory = new RecordingLoggerFactory();
        _ = new LogsInCtorViaFactory(factory);
        Assert.AreEqual(1, factory.Logger.Entries.Count);
    }

    [TestMethod]
    public void LoggingFromTwoLevelDerivedConstructor_BothMessagesRecorded()
    {
        var factory = new RecordingLoggerFactory();
        _ = new DerivedLogsInCtor(factory);
        // Base class ctor logs "ctor-via-factory", derived class ctor logs "derived-ctor"
        Assert.AreEqual(2, factory.Logger.Entries.Count);
    }

    #endregion

    #region All log-level methods

    [TestMethod]
    public void AllLogLevelMethods_DoNotThrow()
    {
        var recording = new RecordingLogger();
        var sut = new AllLevelLogger(recording);
        sut.LogAll();
        Assert.AreEqual(6, recording.Entries.Count);
    }

    [TestMethod]
    public void AllLogLevelMethods_LogCorrectLevels()
    {
        var recording = new RecordingLogger();
        var sut = new AllLevelLogger(recording);
        sut.LogAll();

        var levels = recording.Entries;
        Assert.AreEqual(LogLevel.Trace,       levels[0].Level);
        Assert.AreEqual(LogLevel.Debug,       levels[1].Level);
        Assert.AreEqual(LogLevel.Information, levels[2].Level);
        Assert.AreEqual(LogLevel.Warning,     levels[3].Level);
        Assert.AreEqual(LogLevel.Error,       levels[4].Level);
        Assert.AreEqual(LogLevel.Critical,    levels[5].Level);
    }

    #endregion
}
