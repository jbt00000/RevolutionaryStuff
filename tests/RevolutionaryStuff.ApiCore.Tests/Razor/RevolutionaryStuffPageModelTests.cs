using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.ApiCore.Razor;
using static RevolutionaryStuff.ApiCore.Razor.RevolutionaryStuffPageModel;

namespace RevolutionaryStuff.ApiCore.Tests.Razor;

[TestClass]
public class RevolutionaryStuffPageModelTests
{
    #region Test helpers

    private sealed class LogEntry
    {
        public LogLevel Level { get; init; }
        public string Message { get; init; }
    }

    private sealed class RecordingLogger : ILogger
    {
        public IReadOnlyList<LogEntry> Entries => EntryList;
        private readonly List<LogEntry> EntryList = [];

        public IDisposable BeginScope<TState>(TState state) => NullLogger.Instance.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => EntryList.Add(new LogEntry { Level = logLevel, Message = formatter(state, exception) });
    }

    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        public readonly RecordingLogger Logger = new();
        public ILogger CreateLogger(string categoryName) => Logger;
        public void AddProvider(ILoggerProvider provider) { }
        public void Dispose() { }
    }

    private sealed class ConcretePageModel : RevolutionaryStuffPageModel
    {
        public ConcretePageModel(RevolutionaryStuffPageModelConstructorArgs args) : base(args) { }

        public ILogger PublicLogger => Logger;

        public void PublicLog(LogLevel level, string message) => Log(level, message);
        public void PublicLogTrace(string message) => LogTrace(message);
        public void PublicLogDebug(string message) => LogDebug(message);
        public void PublicLogInformation(string message) => LogInformation(message);
        public void PublicLogWarning(string message) => LogWarning(message);
        public void PublicLogError(string message) => LogError(message);
        public void PublicLogErrorWithException(Exception ex, string message) => LogError(ex, message);
        public void PublicLogException(Exception ex) => LogException(ex);
        public void PublicLogCritical(string message) => LogCritical(message);
        public void PublicLogCriticalWithException(Exception ex, string message) => LogCritical(ex, message);
        public void PublicLogCriticalWithEventId(EventId eventId, Exception ex, string message) => LogCritical(eventId, ex, message);

        public IDisposable PublicCreateLogRegion(LogLevel level) => CreateLogRegion(level, "test-region");
        public IDisposable PublicCreateLogRegionDefault() => CreateLogRegion("test-region");
        public IDisposable PublicLogScopedProperty(string name, object value) => LogScopedProperty(name, value);
    }

    private static RevolutionaryStuffPageModelConstructorArgs MakeArgs(ILoggerFactory factory)
        => new(factory);

    #endregion

    #region Construction

    [TestMethod]
    public void Constructor_WithValidLoggerFactory_DoesNotThrow()
    {
        _ = new ConcretePageModel(MakeArgs(NullLoggerFactory.Instance));
    }

    [TestMethod]
    public void Constructor_NullArgs_ThrowsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(() => new ConcretePageModel(null));

    [TestMethod]
    public void Constructor_ArgsWithNullLoggerFactory_ThrowsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(() => new ConcretePageModel(MakeArgs(null)));

    #endregion

    #region Logger property

    [TestMethod]
    public void Logger_IsNotNull()
    {
        var sut = new ConcretePageModel(MakeArgs(NullLoggerFactory.Instance));
        Assert.IsNotNull(sut.PublicLogger);
    }

    [TestMethod]
    public void Logger_ReturnsSameInstanceOnRepeatedAccess()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        Assert.AreSame(sut.PublicLogger, sut.PublicLogger);
    }

    [TestMethod]
    public void Logger_UsesLoggerFactory_ToCreateTypedLogger()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        Assert.IsNotNull(sut.PublicLogger);
        Assert.AreSame(factory.Logger, sut.PublicLogger);
    }

    #endregion

    #region Log-level helpers

    [TestMethod]
    public void LogTrace_IsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogTrace("trace-msg");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Trace, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogDebug_IsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogDebug("debug-msg");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Debug, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogInformation_IsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogInformation("info-msg");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Information, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogWarning_IsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogWarning("warn-msg");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Warning, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogError_IsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogError("error-msg");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Error, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogErrorWithException_IsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogErrorWithException(new InvalidOperationException("oops"), "error-with-ex");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Error, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogException_IsRecordedAtErrorLevel()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogException(new InvalidOperationException("boom"));
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Error, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogCritical_IsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogCritical("critical-msg");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Critical, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogCriticalWithException_IsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogCriticalWithException(new InvalidOperationException("critical-ex"), "critical-with-ex");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Critical, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogCriticalWithEventId_IsRecorded()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLogCriticalWithEventId(new EventId(42, "test"), new InvalidOperationException("ev"), "critical-event");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Critical, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void Log_WithExplicitLevel_IsRecordedAtThatLevel()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));
        sut.PublicLog(LogLevel.Warning, "explicit-level-msg");
        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Warning, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void AllLogLevelHelpers_RecordCorrectLevels()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcretePageModel(MakeArgs(factory));

        sut.PublicLogTrace("t");
        sut.PublicLogDebug("d");
        sut.PublicLogInformation("i");
        sut.PublicLogWarning("w");
        sut.PublicLogError("e");
        sut.PublicLogCritical("c");

        var levels = factory.Logger.Entries.Select(e => e.Level).ToList();
        CollectionAssert.AreEqual(
            new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical },
            levels);
    }

    #endregion

    #region CreateLogRegion

    [TestMethod]
    public void CreateLogRegion_WithLevel_ReturnsNonNullDisposable()
    {
        var sut = new ConcretePageModel(MakeArgs(NullLoggerFactory.Instance));
        using var region = sut.PublicCreateLogRegion(LogLevel.Information);
        Assert.IsNotNull(region);
    }

    [TestMethod]
    public void CreateLogRegion_Default_ReturnsNonNullDisposable()
    {
        var sut = new ConcretePageModel(MakeArgs(NullLoggerFactory.Instance));
        using var region = sut.PublicCreateLogRegionDefault();
        Assert.IsNotNull(region);
    }

    #endregion

    #region LogScopedProperty

    [TestMethod]
    public void LogScopedProperty_ReturnsNonNullDisposable()
    {
        var sut = new ConcretePageModel(MakeArgs(NullLoggerFactory.Instance));
        using var scope = sut.PublicLogScopedProperty("myProp", "myValue");
        Assert.IsNotNull(scope);
    }

    #endregion
}
