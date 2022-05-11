using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class TaskHelpersTests
{
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
        Assert.IsTrue(maxConcurrent > MAX_AT_ONCE/2);
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
            Stuff.Noop(ex);
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
            Stuff.Noop(ex);
        }
    }

    [TestMethod]
    public Task TaskHelpersWhenAllWorksWithEmptyInputsAsync()
        => TaskHelpers.WhenAll(new Task[0]);

    [TestMethod]
    public Task TaskHelpersWhenAllWorksWithNullInputsAsync()
        => TaskHelpers.WhenAll((Task[])null);

    [TestMethod]
    public async Task TaskHelpersWhenAllTRetWorksWithEmptyInputsAsync()
    {
        var res = await TaskHelpers.WhenAll(new Task<int>[0]);
        Assert.AreEqual(0, res.Length);
    }

    [TestMethod]
    public async Task TaskHelpersWhenAllTRetWorksWithNullInputsAsync()
    {
        var res = await TaskHelpers.WhenAll<int>(null);
        Assert.AreEqual(0, res.Length);
    }

}
