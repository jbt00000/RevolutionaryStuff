using System;
using System.Diagnostics;
using System.Threading;

namespace RevolutionaryStuff.Core
{
    /// <summary>
    /// This class provides an abstract implementation of IDisposable
    /// That makes it easier for subclasses to handle dispose
    /// </summary>
    public abstract class BaseDisposable : IDisposable
    {
        #region Constructors

        ~BaseDisposable()
        {
            MyDispose(false);
        }

        #endregion

        private int IsDisposed_p;

        /// <summary>
        /// Returns true if dispose has been called
        /// </summary>
        protected bool IsDisposed
        {
            [DebuggerStepThrough]
            get { return IsDisposed_p > 0; }
        }

        protected void CheckNotDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException("This object was already disposed");
        }

        #region IDisposable Members

        public void Dispose()
        {
            MyDispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void MyDispose(bool disposing)
        {
            if (1 != Interlocked.Increment(ref IsDisposed_p)) return;
            OnDispose(disposing);
        }

        /// <summary>
        /// Override this function to handle calls to dispose.
        /// This will only get called once
        /// </summary>
        /// <param name="disposing">True when the object is being disposed, 
        /// false if it is being destructed</param>
        protected virtual void OnDispose(bool disposing)
        {
        }
    }
}