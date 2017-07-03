using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RevolutionaryStuff.Core.Threading
{
    public class WorkQueue : BaseDisposable
    {
        protected List<Thread> Threads { get; } = new List<Thread>();
        protected ConcurrentQueue<Action> Queue = new ConcurrentQueue<Action>();
        private ManualResetEvent PleaseStop = new ManualResetEvent(false);
        private ManualResetEvent IsBored = new ManualResetEvent(true);
        private WaitHandle[] StopSnoozingHandles;
        

        protected int MaxThreads { get; }
        protected string PoolName { get; }

        public WorkQueue(int maxThreads, string poolName = null)
        {
            MaxThreads = maxThreads;
            PoolName = Stuff.CoalesceStrings(poolName, this.GetType().Name);
            StopSnoozingHandles = new WaitHandle[] { PleaseStop, IsBored };
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
            Queue.Enqueue(a);
            IsBored.Set();
        }

        public int Count => Queue.Count;

        public bool IsFullyLoaded => Count > 0;

        public bool IsEmpty => BusyThreads == 0 && Count==0;

        public bool IsBusy => BusyThreads > 0 || Count > 0;

        private int BusyThreads_p;

        public int BusyThreads => BusyThreads_p;

        public int FreeThreads => Count > 0 ? 0 : MaxThreads - BusyThreads;

        public void WaitTillDone()
        {
            for (;;)
            {
                IsBored.WaitOne();
                if (!IsBusy) return;
            }
        }

        protected virtual void WorkLoop()
        {
            for (;;)
            {
                IsBored.Reset();
                Interlocked.Increment(ref BusyThreads_p);
                try
                {
                    Action a;
                    if (Queue.TryDequeue(out a))
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
                if (0 == WaitHandle.WaitAny(this.StopSnoozingHandles)) break;
            }
        }

        public void Flush()
        {
            Action a;
            while (Queue.TryDequeue(out a)) ;
        }

        protected override void OnDispose(bool disposing)
        {
            PleaseStop.Set();
            while (IsBusy) Thread.Sleep(100);
            IsBored.Set();
            base.OnDispose(disposing);
        }
    }
}
