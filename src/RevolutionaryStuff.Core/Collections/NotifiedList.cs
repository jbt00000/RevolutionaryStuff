using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace RevolutionaryStuff.Core.Collections
{
    /// <summary>
    /// A list that supports IEvented so the outside world
    /// can tell when changes occur
    /// </summary>
    /// <typeparam name="T">The type of element being stored in the list</typeparam>
    public class NotifiedList<T> : IList<T>, INotifyCollection<T>
    {
        /// <summary>
        /// The underlying storage
        /// </summary>
        protected readonly List<T> Inner;

        #region Constructors

        /// <summary>
        /// Construct me
        /// </summary>
        public NotifiedList()
            : this(null)
        {
        }

        /// <summary>
        /// Construct me
        /// </summary>
        /// <param name="initialData">The initialization data</param>
        private NotifiedList(IEnumerable<T> initialData)
        {
            if (initialData == null)
            {
                Inner = new List<T>();
            }
            else
            {
                Inner = new List<T>(initialData);
            }
        }

        #endregion

        #region INotifyCollection<T> Members

        public event EventHandler<EventArgs<IEnumerable<T>>> Added;
        public event EventHandler<EventArgs<IEnumerable<T>>> Removed;
        public event EventHandler Changed;

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return Inner.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (IsReadOnly) throw new ReadOnlyException();
            Inner.Insert(index, item);
            OnAdded(item);
        }

        public void RemoveAt(int index)
        {
            if (IsReadOnly) throw new ReadOnlyException();
            T item = this[index];
            Inner.RemoveAt(index);
            OnRemoved(item);
        }

        public T this[int index]
        {
            [DebuggerStepThrough]
            get { return Inner[index]; }
            set
            {
                if (IsReadOnly) throw new ReadOnlyException();
                T old = this[index];
                Inner[index] = value;
                OnRemoved(old);
                OnAdded(value);
            }
        }

        public void Add(T item)
        {
            if (IsReadOnly) throw new ReadOnlyException();
            Inner.Add(item);
            OnAdded(item);
        }

        public void Clear()
        {
            if (IsReadOnly) throw new ReadOnlyException();
            if (Inner.Count == 0) return;
            T[] removed = null;
            if (Removed != null)
            {
                removed = new T[Inner.Count];
                Inner.CopyTo(removed, 0);
            }
            Inner.Clear();
            OnRemoved(removed);
        }

        public bool Contains(T item)
        {
            return Inner.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Inner.CopyTo(array, arrayIndex);
        }

        public virtual int Count
        {
            [DebuggerStepThrough]
            get { return Inner.Count; }
        }

        public bool IsReadOnly
        {
            [DebuggerStepThrough]
            get; [DebuggerStepThrough]
            set;
        }

        public bool Remove(T item)
        {
            if (IsReadOnly) throw new ReadOnlyException();
            if (Inner.Remove(item))
            {
                OnRemoved(item);
                return true;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

        #endregion

        public IList<T> AsReadOnly()
        {
            if (IsReadOnly) throw new ReadOnlyException();
            var el = new NotifiedList<T>(Inner);
            el.IsReadOnly = true;
            return el;
        }

        public void AddRange(IEnumerable<T> stuff)
        {
            if (IsReadOnly) throw new ReadOnlyException();
            Inner.AddRange(stuff);
            OnAdded(stuff);
        }

        public T[] ToArray()
        {
            return Inner.ToArray();
        }

        #region IEvented Helpers

        private void OnAdded(params T[] added)
        {
            OnAdded((IEnumerable<T>)added);
        }

        protected virtual void OnAdded(IEnumerable<T> added)
        {
            Added?.Invoke(this, new EventArgs<IEnumerable<T>>(added));
            OnChanged();
        }

        private void OnRemoved(T removed)
        {
            OnRemoved(new[] { removed });
        }

        protected virtual void OnRemoved(IList<T> removed)
        {
            if (null != Removed)
            {
                var data = new T[removed.Count];
                removed.CopyTo(data, 0);
                Removed(this, new EventArgs<IEnumerable<T>>(removed));
            }
            OnChanged();
        }

        protected virtual void OnChanged()
            => DelegateHelpers.SafeInvoke(Changed, this);

        #endregion
    }
}