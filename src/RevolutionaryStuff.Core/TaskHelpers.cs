using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core
{
    public static class TaskHelpers
    {
        public static TResult ExecuteSynchronously<TResult>(this Task<TResult> task)
        {
            if (!task.IsCompleted)
            {
                try
                {
                    Task.WaitAll(task);
                }
                catch (AggregateException ae)
                {
                    throw ae.InnerException;
                }
            }
            return task.Result;
        }

        public static void ExecuteSynchronously(this Task task)
        {
            if (!task.IsCompleted)
            {
                try
                {
                    Task.WaitAll(task);
                }
                catch (AggregateException ae)
                {
                    throw ae.InnerException;
                }
            }
        }

        public static TResult GetNonTaskResult<TResult>(this Task t, TResult missing = default)
        {
            object o = t;
Again:
            var ot = o.GetType();
            if (ot == typeof(TResult)) return (TResult)o;
            var pi = ot.GetProperty("Result");
            if (pi == null) return missing;
            o = pi.GetValue(o);
            if (o == null) return missing;
            goto Again;
        }

        public static async Task<IList<Task>> TaskWhenAllForEachAsync<TItem>(IEnumerable<TItem> items, Func<TItem, Task> taskCreator, int maxAtOnce = 2, [CallerMemberName] string caller = null, bool throwAggregatedException = false)
        {
            Requires.NonNull(items, nameof(items));
            Requires.NonNull(taskCreator, nameof(taskCreator));
            Requires.Positive(maxAtOnce, nameof(maxAtOnce));

            var tasks = new List<Task>();
            long outstanding = 0;
            foreach (var item in items)
            {
                while (Interlocked.Read(ref outstanding) >= maxAtOnce)
                {
                    await Task.Delay(10);
                }
                Interlocked.Increment(ref outstanding);
                Debug.WriteLine($"{nameof(TaskWhenAllForEachAsync)} from {caller}: Tot Started = {tasks.Count}; Outstanding = {outstanding}");
                var t = taskCreator(item);
                var tDone = t.ContinueWith(a => { Interlocked.Decrement(ref outstanding); return Task.CompletedTask; });
                tasks.Add(t);
            }
            while (Interlocked.Read(ref outstanding) > 0)
            {
                await Task.Delay(10);
            }
            Debug.WriteLine($"{nameof(TaskWhenAllForEachAsync)} from {caller}: Tot Started = {tasks.Count}; Outstanding = {outstanding}");
            Debug.Assert(outstanding == 0);
            if (throwAggregatedException)
            {
                var exceptions = tasks.Where(z => z.Exception != null).Select(z => z.Exception).ToList();
                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
            }
            return tasks;
        }

        public static void TaskWaitAllForEach<TSource>(IEnumerable<TSource> items, Func<TSource, Task> body, [CallerMemberName] string caller = null)
            => TaskWhenAllForEachAsync(items, body, int.MaxValue, caller).ExecuteSynchronously();
    }
}
