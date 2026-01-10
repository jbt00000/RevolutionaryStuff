using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class TaskHelpersTests
{
    #region Existing Tests

    [TestMethod]
    public async Task TaskWhenAllForEachWorkingConcurrentlyTestAsync()
    {
        var items = new List<int>();
        for (var z = 0; z < 200; ++z)
        {
            items.Add(z);
        }
        var threadIds = new HashSet<int>();
        var concurrent = 0;
        var maxConcurrent = 0;
        await TaskHelpers.TaskWhenAllForEachAsync(items, async item =>
        {
            using (new TraceRegion($"{nameof(TaskWhenAllForEachWorkingConcurrentlyTestAsync)} Item={item}"))
            {
                Interlocked.Increment(ref concurrent);
                maxConcurrent = Stuff.Max(concurrent, maxConcurrent);
                await Task.Delay(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                Interlocked.Decrement(ref concurrent);
            }
        }, MAX_AT_ONCE);
        Assert.IsTrue(maxConcurrent > MAX_AT_ONCE / 2);
        Assert.IsTrue(maxConcurrent <= MAX_AT_ONCE);
        Assert.IsTrue(threadIds.Count > 2);
    }

    private const int MAX_AT_ONCE = 20;

    [TestMethod]
    public async Task TaskWhenAllFailsWithEmptyInputsAsync()
    {
        try
        {
            await Task.WhenAll(new Task[0]);
        }
        catch (Exception ex)
        {
            Stuff.NoOp(ex);
        }
    }

    [TestMethod]
    public async Task TaskWhenAllTRetFailsWithEmptyInputsAsync()
    {
        try
        {
            var res = await Task.WhenAll(new Task<int>[0]);
            Assert.AreEqual(0, res.Length);
        }
        catch (Exception ex)
        {
            Stuff.NoOp(ex);
        }
    }

    #endregion

    #region UntilCancelledAsync Tests

    [TestMethod]
    public async Task UntilCancelledAsync_WhenCancelled_CompletesWithCancellation()
    {
        var cts = new CancellationTokenSource();
        var task = cts.Token.UntilCancelledAsync();

        Assert.IsFalse(task.IsCompleted);

        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    [TestMethod]
    public async Task UntilCancelledAsync_AlreadyCancelled_CompletesImmediately()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = cts.Token.UntilCancelledAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    #endregion

    #region ExecuteSynchronously Tests

    [TestMethod]
    public void ExecuteSynchronously_CompletedTask_ReturnsResult()
    {
        var task = Task.FromResult(42);
        var result = task.ExecuteSynchronously();
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ExecuteSynchronously_AsyncTask_ReturnsResult()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(100);
            return "hello";
        });

        var result = task.ExecuteSynchronously();
        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void ExecuteSynchronously_FaultedTask_ThrowsException()
    {
        Func<int> throwingFunc = () => throw new InvalidOperationException("Test exception");
        var task = Task.Run(throwingFunc);

        Assert.Throws<InvalidOperationException>(() => task.ExecuteSynchronously());
    }

    [TestMethod]
    public void ExecuteSynchronously_NonGeneric_CompletesSuccessfully()
    {
        var completed = false;
        var task = Task.Run(async () =>
        {
            await Task.Delay(100);
            completed = true;
        });

        task.ExecuteSynchronously();
        Assert.IsTrue(completed);
    }

    [TestMethod]
    public void ExecuteSynchronously_NonGeneric_FaultedTask_ThrowsException()
    {
        var task = Task.Run(() => throw new InvalidOperationException("Test exception"));

        Assert.Throws<InvalidOperationException>(() => task.ExecuteSynchronously());
    }

    #endregion

    #region GetNonTaskResult Tests

    [TestMethod]
    public void GetNonTaskResult_SimpleTask_ReturnsResult()
    {
        var task = Task.FromResult(42);
        var result = task.GetNonTaskResult<int>();
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void GetNonTaskResult_TaskOfTask_UnwrapsToResult()
    {
        var innerTask = Task.FromResult(42);
        var task = Task.FromResult(innerTask);
        var result = task.GetNonTaskResult<int>();
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void GetNonTaskResult_NonMatchingType_ReturnsDefault()
    {
        var task = Task.FromResult("hello");
        var result = task.GetNonTaskResult<int>(missing: -1);
        Assert.AreEqual(-1, result);
    }

    #endregion

    #region TaskWhenAllForEachAsync Additional Tests

    [TestMethod]
    public async Task TaskWhenAllForEachAsync_EmptyCollection_Succeeds()
    {
        var items = new List<int>();
        var processed = new List<int>();

        var tasks = await TaskHelpers.TaskWhenAllForEachAsync(items, async item =>
        {
            await Task.Delay(10);
            processed.Add(item);
        });

        Assert.AreEqual(0, tasks.Count);
        Assert.AreEqual(0, processed.Count);
    }

    [TestMethod]
    public async Task TaskWhenAllForEachAsync_NullItems_ThrowsException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await TaskHelpers.TaskWhenAllForEachAsync<int>(null, async item => await Task.Delay(10));
        });
    }

    [TestMethod]
    public async Task TaskWhenAllForEachAsync_NullTaskCreator_ThrowsException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await TaskHelpers.TaskWhenAllForEachAsync(new[] { 1, 2, 3 }, null);
        });
    }

    [TestMethod]
    public async Task TaskWhenAllForEachAsync_ZeroMaxAtOnce_ThrowsException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await TaskHelpers.TaskWhenAllForEachAsync(new[] { 1, 2, 3 }, async item => await Task.Delay(10), maxAtOnce: 0);
        });
    }

    [TestMethod]
    public async Task TaskWhenAllForEachAsync_NegativeMaxAtOnce_ThrowsException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await TaskHelpers.TaskWhenAllForEachAsync(new[] { 1, 2, 3 }, async item => await Task.Delay(10), maxAtOnce: -1);
        });
    }

    [TestMethod]
    public async Task TaskWhenAllForEachAsync_ProcessesAllItems()
    {
        var items = Enumerable.Range(1, 10).ToList();
        var processed = new List<int>();
        var lockObj = new object();

        await TaskHelpers.TaskWhenAllForEachAsync(items, async item =>
        {
            await Task.Delay(10);
            lock (lockObj)
            {
                processed.Add(item);
            }
        }, maxAtOnce: 3);

        Assert.AreEqual(10, processed.Count);
        Assert.IsTrue(items.All(i => processed.Contains(i)));
    }

    [TestMethod]
    public async Task TaskWhenAllForEachAsync_ThrowAggregateException_CollectsExceptions()
    {
        var items = new[] { 1, 2, 3, 4, 5 };

        var aggregateEx = await Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await TaskHelpers.TaskWhenAllForEachAsync(items, async item =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException($"Error {item}");
            }, maxAtOnce: 2, throwAggregatedException: true);
        });

        Assert.AreEqual(5, aggregateEx.InnerExceptions.Count);
    }

    [TestMethod]
    public async Task TaskWhenAllForEachAsync_NoThrow_ReturnsTasksWithExceptions()
    {
        var items = new[] { 1, 2, 3 };

        var tasks = await TaskHelpers.TaskWhenAllForEachAsync(items, async item =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException($"Error {item}");
        }, maxAtOnce: 2, throwAggregatedException: false);

        Assert.AreEqual(3, tasks.Count);
        Assert.IsTrue(tasks.All(t => t.IsFaulted));
    }

    #endregion

    #region TaskWaitAllForEach Tests

    [TestMethod]
    public void TaskWaitAllForEach_ProcessesAllItems()
    {
        var items = new[] { 1, 2, 3, 4, 5 };
        var processed = new List<int>();
        var lockObj = new object();

        TaskHelpers.TaskWaitAllForEach(items, async item =>
        {
            await Task.Delay(10);
            lock (lockObj)
            {
                processed.Add(item);
            }
        });

        Assert.AreEqual(5, processed.Count);
    }

    [TestMethod]
    public void TaskWaitAllForEach_BlocksUntilComplete()
    {
        var completed = false;
        var items = new[] { 1 };

        TaskHelpers.TaskWaitAllForEach(items, async item =>
        {
            await Task.Delay(100);
            completed = true;
        });

        Assert.IsTrue(completed);
    }

    #endregion

    #region TaskWhenAllThatAreNotNull Tests

    [TestMethod]
    public async Task TaskWhenAllThatAreNotNull_AllNonNull_WaitsForAll()
    {
        var completed1 = false;
        var completed2 = false;

        var task1 = Task.Run(async () =>
        {
            await Task.Delay(50);
            completed1 = true;
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(50);
            completed2 = true;
        });

        await TaskHelpers.TaskWhenAllThatAreNotNull(task1, task2);

        Assert.IsTrue(completed1);
        Assert.IsTrue(completed2);
    }

    [TestMethod]
    public async Task TaskWhenAllThatAreNotNull_SomeNull_IgnoresNulls()
    {
        var completed = false;

        var task1 = Task.Run(async () =>
        {
            await Task.Delay(50);
            completed = true;
        });

        await TaskHelpers.TaskWhenAllThatAreNotNull(task1, null, null);

        Assert.IsTrue(completed);
    }

    [TestMethod]
    public async Task TaskWhenAllThatAreNotNull_AllNull_CompletesImmediately()
    {
        await TaskHelpers.TaskWhenAllThatAreNotNull(null, null, null);
        // Should complete without error
    }

    [TestMethod]
    public async Task TaskWhenAllThatAreNotNull_EmptyArray_CompletesImmediately()
    {
        await TaskHelpers.TaskWhenAllThatAreNotNull();
        // Should complete without error
    }

    #endregion

    #region ContinueWithToIList Tests

    [TestMethod]
    public async Task ContinueWithToIList_ConvertsReadOnlyToMutable()
    {
        var readOnlyList = new List<int> { 1, 2, 3 }.AsReadOnly();
        var task = Task.FromResult((IReadOnlyList<int>)readOnlyList);

        var result = await task.ContinueWithToIList();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(1, result[0]);
        Assert.AreEqual(2, result[1]);
        Assert.AreEqual(3, result[2]);

        // Verify it's mutable
        result.Add(4);
        Assert.AreEqual(4, result.Count);
    }

    [TestMethod]
    public async Task ContinueWithToIList_EmptyList_ReturnsEmptyMutableList()
    {
        var readOnlyList = new List<string>().AsReadOnly();
        var task = Task.FromResult((IReadOnlyList<string>)readOnlyList);

        var result = await task.ContinueWithToIList();

        Assert.AreEqual(0, result.Count);

        // Verify it's mutable
        result.Add("test");
        Assert.AreEqual(1, result.Count);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public async Task Integration_CancellationWithParallelWork()
    {
        var cts = new CancellationTokenSource();
        var processedCount = 0;

        var workTask = TaskHelpers.TaskWhenAllForEachAsync(
            Enumerable.Range(1, 100),
            async item =>
            {
                if (cts.Token.IsCancellationRequested)
                    return;

                await Task.Delay(50, cts.Token);
                Interlocked.Increment(ref processedCount);
            },
            maxAtOnce: 10);

        // Cancel after a short delay
        await Task.Delay(200);
        cts.Cancel();

        // Wait for work to complete (with cancellation)
        try
        {
            await workTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Some items should have been processed before cancellation
        Assert.IsTrue(processedCount > 0);
        Assert.IsTrue(processedCount < 100);
    }

    #endregion
}
