﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RevolutionaryStuff.Core;

public static class TaskHelpers
{
    private static Task AwaitCancellation(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();

        cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

        return tcs.Task;
    }

    public static Task UntilCancelledAsync(this CancellationToken cancellationToken)
        => AwaitCancellation(cancellationToken);

    public static TResult ExecuteSynchronously<TResult>(this Task<TResult> task)
    {
        if (task.IsCompleted)
        {
            if (task.Exception != null)
            {
                throw task.Exception;
            }
        }
        else
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
        if (task.IsCompleted)
        {
            if (task.Exception != null)
            {
                throw task.Exception;
            }
        }
        else
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
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(taskCreator);
        Requires.Positive(maxAtOnce);

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

    /*
    internal static Task WhenAll(IEnumerable<Task> tasks)
    {
        if (tasks == null) return Task.CompletedTask;
        var ts = tasks.ToList();
        return ts.Count == 0 ? Task.CompletedTask : Task.WhenAll(ts);
    }

    internal static Task<TRet[]> WhenAll<TRet>(IEnumerable<Task<TRet>> tasks)
    {
        if (tasks == null) return Task.FromResult(new TRet[0]);
        var ts = tasks.ToList();
        return ts.Count == 0 ? Task.FromResult(new TRet[0]) : Task.WhenAll(ts);
    }
    */

    public static Task TaskWhenAllThatAreNotNull(params Task[] tasks)
        => Task.WhenAll(tasks.WhereNotNull());
}
