using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides utility methods for working with tasks, async/await patterns, and parallel execution.
/// Includes methods for synchronous execution, cancellation token handling, and controlled concurrency.
/// </summary>
public static class TaskHelpers
{
    private static Task AwaitCancellation(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();

        cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

        return tcs.Task;
    }

    /// <summary>
    /// Creates a task that completes when the cancellation token is cancelled.
    /// Useful for await-ing cancellation in async methods.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to monitor.</param>
    /// <returns>A task that completes (with cancellation) when the token is cancelled.</returns>
    /// <example>
    /// <code>
    /// await cancellationToken.UntilCancelledAsync();
    /// // Execution continues here after cancellation
    /// </code>
    /// </example>
    public static Task UntilCancelledAsync(this CancellationToken cancellationToken)
        => AwaitCancellation(cancellationToken);

    /// <summary>
    /// Executes a task synchronously and returns its result.
    /// Blocks the calling thread until the task completes.
    /// </summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    /// <param name="task">The task to execute synchronously.</param>
    /// <returns>The result of the task.</returns>
    /// <exception cref="Exception">The inner exception from the task if it faulted.</exception>
    /// <remarks>
    /// Warning: This can cause deadlocks if used in UI or ASP.NET contexts.
    /// Prefer async/await when possible.
    /// </remarks>
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

    /// <summary>
    /// Executes a task synchronously without returning a result.
    /// Blocks the calling thread until the task completes.
    /// </summary>
    /// <param name="task">The task to execute synchronously.</param>
    /// <exception cref="Exception">The inner exception from the task if it faulted.</exception>
    /// <remarks>
    /// Warning: This can cause deadlocks if used in UI or ASP.NET contexts.
    /// Prefer async/await when possible.
    /// </remarks>
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

    /// <summary>
    /// Extracts the non-task result from a potentially nested Task&lt;Task&lt;T&gt;&gt; structure.
    /// Unwraps nested tasks recursively to get the actual result value.
    /// </summary>
    /// <typeparam name="TResult">The type of the expected result.</typeparam>
    /// <param name="t">The task to unwrap.</param>
    /// <param name="missing">The default value to return if the result cannot be extracted.</param>
    /// <returns>The unwrapped result value, or <paramref name="missing"/> if not found.</returns>
    /// <remarks>
    /// Uses reflection to navigate nested Task.Result properties.
    /// </remarks>
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

    /// <summary>
    /// Executes tasks in parallel with controlled concurrency.
    /// Limits the number of tasks running concurrently to prevent resource exhaustion.
    /// </summary>
    /// <typeparam name="TItem">The type of items to process.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="taskCreator">Function that creates a task for each item.</param>
    /// <param name="maxAtOnce">Maximum number of tasks to run concurrently. Defaults to 2.</param>
    /// <param name="caller">The name of the calling method (automatically captured).</param>
    /// <param name="throwAggregatedException">
    /// If <c>true</c>, throws an AggregateException containing all task exceptions.
    /// If <c>false</c>, exceptions are not thrown but tasks may still be faulted.
    /// </param>
    /// <returns>A list of all created tasks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> or <paramref name="taskCreator"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxAtOnce"/> is not positive.</exception>
    /// <exception cref="AggregateException">Thrown when <paramref name="throwAggregatedException"/> is true and any tasks faulted.</exception>
    /// <example>
    /// <code>
    /// var items = Enumerable.Range(1, 100);
    /// await TaskHelpers.TaskWhenAllForEachAsync(items, async item =>
    /// {
    ///     await ProcessItemAsync(item);
    /// }, maxAtOnce: 10);
    /// </code>
    /// </example>
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

    /// <summary>
    /// Executes tasks for each item synchronously (blocking), with unlimited concurrency.
    /// This is a synchronous wrapper around <see cref="TaskWhenAllForEachAsync{TItem}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of items to process.</typeparam>
    /// <param name="items">The collection of items to process.</param>
    /// <param name="body">Function that creates a task for each item.</param>
    /// <param name="caller">The name of the calling method (automatically captured).</param>
    /// <remarks>
    /// Uses int.MaxValue for maxAtOnce, effectively allowing unlimited concurrency.
    /// Warning: This blocks the calling thread. Prefer async methods when possible.
    /// </remarks>
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

    /// <summary>
    /// Waits for all non-null tasks to complete.
    /// Null tasks in the array are ignored.
    /// </summary>
    /// <param name="tasks">An array of tasks, which may contain nulls.</param>
    /// <returns>A task that completes when all non-null tasks have completed.</returns>
    /// <remarks>
    /// This is useful when building task arrays conditionally where some may be null.
    /// </remarks>
    /// <example>
    /// <code>
    /// var task1 = condition1 ? DoWork1Async() : null;
    /// var task2 = condition2 ? DoWork2Async() : null;
    /// await TaskHelpers.TaskWhenAllThatAreNotNull(task1, task2);
    /// </code>
    /// </example>
    public static Task TaskWhenAllThatAreNotNull(params Task[] tasks)
        => Task.WhenAll(tasks.WhereNotNull());

    /// <summary>
    /// Converts a Task&lt;IReadOnlyList&lt;T&gt;&gt; to a Task&lt;IList&lt;T&gt;&gt;.
    /// Useful when you need a mutable list interface from a readonly list result.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <param name="task">The task returning a readonly list.</param>
    /// <returns>A task returning a mutable list with the same items.</returns>
    /// <remarks>
    /// The returned list is a copy of the original items.
    /// </remarks>
    public static async Task<IList<T>> ContinueWithToIList<T>(this Task<IReadOnlyList<T>> task)
    {
        var items = await task;
        return ToIList<T>(items);
    }
    
    /// <summary>
    /// Converts a readonly list to a mutable list.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <param name="l">The readonly list to convert.</param>
    /// <returns>A new mutable list containing the same items.</returns>
    private static IList<T> ToIList<T>(IReadOnlyList<T> l)
        => [.. l];
}
