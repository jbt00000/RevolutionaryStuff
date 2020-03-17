using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Diagnostics;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class TaskHelpersTests
    {
        [TestMethod]
        public async Task TaskWhenAllForEachWorkingConcurrentlyTestAsync()
        {
            var items = new List<int>();
            for (int z = 0; z < 100; ++z)
            {
                items.Add(z);
            }
            var threadIds = new HashSet<int>();
            int concurrent = 0;
            int maxConcurrent = 0;
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
            }, 10);
            Assert.IsTrue(maxConcurrent > 8);
            Assert.IsTrue(maxConcurrent <= 10);
            Assert.IsTrue(threadIds.Count > 5);
        }
    }
}
