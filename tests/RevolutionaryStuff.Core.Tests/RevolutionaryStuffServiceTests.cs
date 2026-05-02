using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class RevolutionaryStuffServiceTests
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

    private sealed class ConcreteService : RevolutionaryStuffService
    {
        public ConcreteService(RevolutionaryStuffService.RevolutionaryStuffServiceConstrutorArge args) : base(args) { }

        public bool PublicIsDisposed => IsDisposed;

        public void PublicLogInformation(string message) => LogInformation(message);
        public void PublicLogWarning(string message) => LogWarning(message);
        public void PublicLogError(string message) => LogError(message);

        public Task PublicActAsync(Func<Task> executeAsync) => ActAsync(executeAsync);

        protected override void OnDispose(bool disposing)
        {
            OnDisposedCalledWith = disposing;
            base.OnDispose(disposing);
        }

        public bool? OnDisposedCalledWith { get; private set; }
    }

    private static RevolutionaryStuffService.RevolutionaryStuffServiceConstrutorArge MakeArgs(ILoggerFactory factory)
        => new(factory);

    #endregion

    #region Construction

    [TestMethod]
    public void Constructor_WithValidLoggerFactory_DoesNotThrow()
    {
        var args = MakeArgs(NullLoggerFactory.Instance);
        _ = new ConcreteService(args);
    }

    [TestMethod]
    public void Constructor_NullConstructorArgs_ThrowsNullReferenceException()
        => Assert.ThrowsExactly<NullReferenceException>(() => new ConcreteService(null));

    [TestMethod]
    public void Constructor_ArgsWithNullLoggerFactory_ThrowsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(() => new ConcreteService(MakeArgs(null)));

    #endregion

    #region Dispose / IsDisposed

    [TestMethod]
    public void IsDisposed_BeforeDispose_IsFalse()
    {
        var sut = new ConcreteService(MakeArgs(NullLoggerFactory.Instance));
        Assert.IsFalse(sut.PublicIsDisposed);
    }

    [TestMethod]
    public void IsDisposed_AfterDispose_IsTrue()
    {
        var sut = new ConcreteService(MakeArgs(NullLoggerFactory.Instance));
        sut.Dispose();
        Assert.IsTrue(sut.PublicIsDisposed);
    }

    [TestMethod]
    public void Dispose_CalledMultipleTimes_OnDisposeInvokedOnlyOnce()
    {
        var sut = new ConcreteService(MakeArgs(NullLoggerFactory.Instance));
        sut.Dispose();
        sut.Dispose();
        // OnDisposedCalledWith is only set once — second Dispose must be a no-op
        Assert.IsTrue(sut.OnDisposedCalledWith.HasValue);
    }

    [TestMethod]
    public void Dispose_OnDisposeReceivesDisposingTrue()
    {
        var sut = new ConcreteService(MakeArgs(NullLoggerFactory.Instance));
        sut.Dispose();
        Assert.IsTrue(sut.OnDisposedCalledWith);
    }

    #endregion

    #region Logging via RevolutionaryStuffServiceConstrutorArge

    [TestMethod]
    public void LogInformation_IsRecordedByFactory()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcreteService(MakeArgs(factory));

        sut.PublicLogInformation("hello-info");

        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Information, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogWarning_IsRecordedByFactory()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcreteService(MakeArgs(factory));

        sut.PublicLogWarning("hello-warning");

        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Warning, factory.Logger.Entries[0].Level);
    }

    [TestMethod]
    public void LogError_IsRecordedByFactory()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcreteService(MakeArgs(factory));

        sut.PublicLogError("hello-error");

        Assert.AreEqual(1, factory.Logger.Entries.Count);
        Assert.AreEqual(LogLevel.Error, factory.Logger.Entries[0].Level);
    }

    #endregion

    #region ActAsync

    [TestMethod]
    public async Task ActAsync_ExecutesDelegate()
    {
        var sut = new ConcreteService(MakeArgs(NullLoggerFactory.Instance));
        var executed = false;

        await sut.PublicActAsync(() => { executed = true; return Task.CompletedTask; });

        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task ActAsync_NullDelegate_ThrowsArgumentNullException()
    {
        var sut = new ConcreteService(MakeArgs(NullLoggerFactory.Instance));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => sut.PublicActAsync(null));
    }

    [TestMethod]
    public async Task ActAsync_WhenDelegateThrows_RethrowsException()
    {
        var sut = new ConcreteService(MakeArgs(NullLoggerFactory.Instance));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => sut.PublicActAsync(() => throw new InvalidOperationException("boom")));
    }

    [TestMethod]
    public async Task ActAsync_LogsStartAndCompletion()
    {
        var factory = new RecordingLoggerFactory();
        var sut = new ConcreteService(MakeArgs(factory));

        await sut.PublicActAsync(() => Task.CompletedTask);

        // ActAsync logs "started" and "completed" — at least 2 information entries expected
        Assert.IsTrue(factory.Logger.Entries.Count >= 2);
        Assert.IsTrue(factory.Logger.Entries.All(e => e.Level == LogLevel.Information));
    }

    #endregion
}
