using System;
using System.Threading;

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

        CacheEntry<TVal> ICacher.FindOrCreate<TVal>(string key, Func<string, CacheEntry<TVal>> creator, bool forceCreate, TimeSpan? timeout)
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
                return Inner.FindOrCreate(key, creator, forceCreate, timeout);
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
    }
}
