using System;
using System.Diagnostics;
using System.Threading;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core.Caching
{
    /// <summary>
    /// Thread safe wrapper for any ICache implementation
    /// </summary>
    public class SynchronizedCache<K, D> : ICache<K, D>, IName
    {
        protected readonly ICache<K, D> Inner;
        private readonly string Name;
        private readonly ReaderWriterLockSlim RWL = new ReaderWriterLockSlim();

        #region Constructors

        protected SynchronizedCache()
        {
        }

        public SynchronizedCache(ICache<K, D> inner)
            : this(inner, null)
        {
        }

        public SynchronizedCache(ICache<K, D> inner, string name)
        {
            Requires.NonNull(inner, nameof(inner));

            Inner = inner;
            Name = string.Format("{0}.{1}", StringHelpers.Coalesce(name, inner.GetType().Name), GetHashCode());
        }

        #endregion

        #region ICache<K,D> Members

        public virtual void Add(K key, D data)
        {
            RWL.EnterWriteLock();
            try
            {
                Inner.Add(key, data);
            }
            finally
            {
                RWL.ExitWriteLock();
            }
        }

        public virtual void Remove(K key)
        {
            RWL.EnterWriteLock();
            try
            {
                Inner.Remove(key);
            }
            finally
            {
                RWL.ExitWriteLock();
            }
        }

        public virtual bool Find(K key, out D data)
        {
            bool found = false;
            RWL.EnterReadLock();
            try
            {
                found = Inner.Find(key, out data);
                return found;
            }
            finally
            {
                RWL.ExitReadLock();
                if (found && HitHandler != null)
                {
                    HitHandler.SafeInvoke(this);
                }
                else if (!found && MissHandler != null)
                {
                    MissHandler.SafeInvoke(this);
                }
            }
        }

        public virtual bool Exists(K key)
        {
            RWL.EnterReadLock();
            try
            {
                return Inner.Exists(key);
            }
            finally
            {
                RWL.ExitReadLock();
            }
        }

        void IFlushable.Flush()
        {
            RWL.EnterWriteLock();
            try
            {
                Inner.Flush();
            }
            finally
            {
                RWL.ExitWriteLock();
            }
            try
            {
                FlushHandler?.SafeInvoke(this);
            }
            catch (Exception)
            {
                // TODO: log exception
            }
        }

        int ICache<K, D>.Count
        {
            get
            {
                RWL.EnterReadLock();
                try
                {
                    return Inner.Count;
                }
                finally
                {
                    RWL.ExitReadLock();
                }
            }
        }

        #endregion

        #region IName Members

        string IName.Name
        {
            [DebuggerStepThrough]
            get { return Name; }
        }

        #endregion

        public event EventHandler HitHandler;
        public event EventHandler MissHandler;
        public event EventHandler FlushHandler;

        public override string ToString()
        {
            return string.Format("Synchronized: {0}", Inner);
        }
    }
}
