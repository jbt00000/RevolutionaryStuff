using System;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    public class SynchronizedCacher : ICacher
    {
        private readonly ICacher Inner;

        public SynchronizedCacher(ICacher inner)
        {
            Requires.NonNull(inner, nameof(inner));
            Inner = inner;
        }

        Task<ICacheEntry> ICacher.FindOrCreateEntryAsync(string key, Func<string, Task<ICacheEntry>> asyncCreator, IFindOrCreateEntrySettings settings)
        {
            var lockName = Cache.GetLockKeyName(Inner, key);
            Start:
            object o;
            lock (Cache.LockByKey)
            {
                if (!Cache.LockByKey.TryGetValue(lockName, out o))
                {
                    o = new object();
                    Cache.LockByKey[lockName] = o;
                }
                if (Monitor.TryEnter(o)) goto Run;
            }
            Monitor.Enter(o);
            Monitor.Exit(o);
            goto Start;
            Run:
            try
            {
                return Inner.FindOrCreateEntryAsync(key, asyncCreator, settings);
            }
            finally
            {
                lock (Cache.LockByKey)
                {
                    Cache.LockByKey.Remove(lockName);
                }
                Monitor.Exit(o);
            }
        }

        Task ICacher.RemoveAsync(string key)
        {
            lock (Cache.LockByKey)
            {
                Inner.RemoveAsync(key).ExecuteSynchronously();
            }
            return Task.CompletedTask;
        }
    }
}
