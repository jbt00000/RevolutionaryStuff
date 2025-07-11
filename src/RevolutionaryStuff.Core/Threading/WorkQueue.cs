﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace RevolutionaryStuff.Core.Threading;

public class WorkQueue : BaseDisposable
{
    protected List<Thread> Threads { get; } = [];
    protected ConcurrentQueue<Action> Queue = new();
    private readonly ManualResetEvent PleaseStop = new(false);
    private readonly ManualResetEvent IsBored = new(true);
    private readonly WaitHandle[] StopSnoozingHandles;


    protected int MaxThreads { get; }
    protected string PoolName { get; }

    public WorkQueue(int maxThreads, string poolName = null)
    {
        MaxThreads = maxThreads;
        PoolName = StringHelpers.Coalesce(poolName, GetType().Name);
        StopSnoozingHandles = [PleaseStop, IsBored];
        CreateThread(true);
    }

    protected void CreateThread(bool force)
    {
        if (!force && (BusyThreads < Threads.Count || BusyThreads >= MaxThreads)) return;
        lock (this)
        {
            if (!force && (BusyThreads < Threads.Count || BusyThreads >= MaxThreads)) return;
            var t = new Thread(new ThreadStart(WorkLoop)) { IsBackground = true, Name = $"{PoolName} #{Threads.Count}" };
            Threads.Add(t);
            t.Start();
        }
    }

    public void Enqueue(Action a)
    {
        CheckNotDisposed();
        Queue.Enqueue(a);
        IsBored.Set();
    }

    public int Count => Queue.Count;

    public bool IsFullyLoaded => Count > 0;

    public bool IsEmpty => BusyThreads == 0 && Count == 0;

    public bool IsBusy => BusyThreads > 0 || Count > 0;

    private int BusyThreads_p;

    public int BusyThreads => BusyThreads_p;

    public int FreeThreads => Count > 0 ? 0 : MaxThreads - BusyThreads;

    public void WaitTillDone()
    {
        CheckNotDisposed();
        for (; ; )
        {
            IsBored.WaitOne();
            if (!IsBusy) return;
        }
    }

    protected virtual void WorkLoop()
    {
        for (; ; )
        {
            IsBored.Reset();
            Interlocked.Increment(ref BusyThreads_p);
            try
            {
                if (Queue.TryDequeue(out var a))
                {
                    CreateThread(false);
                    IsBored.Set();
                    try
                    {
                        a();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    continue;
                }
            }
            finally
            {
                Interlocked.Decrement(ref BusyThreads_p);
            }
            if (0 == WaitHandle.WaitAny(StopSnoozingHandles)) break;
        }
    }

    public void Flush()
    {
        CheckNotDisposed();
        while (Queue.TryDequeue(out var a)) ;
    }

    protected override void OnDispose(bool disposing)
    {
        if (disposing)
        {
            PleaseStop.Set();

            // Give threads time to finish
            const int maxWaitTime = 5000; // 5 seconds max wait time
            const int waitInterval = 100;
            var totalWaited = 0;

            while (IsBusy && totalWaited < maxWaitTime)
            {
                Thread.Sleep(waitInterval);
                totalWaited += waitInterval;
            }

            IsBored.Set();

            // Dispose WaitHandles
            PleaseStop.Dispose();
            IsBored.Dispose();

            // Clear the queue
            Flush();
        }

        base.OnDispose(disposing);
    }
}
