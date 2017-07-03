using System;
using System.Collections.Generic;

namespace RevolutionaryStuff.Core.Collections
{
    /// <summary>
    /// This interface describes the changes that are made to an underlying collection
    /// </summary>
    /// <typeparam name="T">The datatype that is being added/removed</typeparam>
    public interface INotifyCollection<T>
    {
        /// <summary>
        /// Fires when items are added.  Iterate through the enumerable to see exactly what was added
        /// </summary>
        event EventHandler<EventArgs<IEnumerable<T>>> Added;

        /// <summary>
        /// Fires when items are remvoed.  Iterate through the enumerable to see exactly what was removed
        /// </summary>
        event EventHandler<EventArgs<IEnumerable<T>>> Removed;

        /// <summary>
        /// Fired when items are either added or removed
        /// </summary>
        event EventHandler Changed;
    }
}
