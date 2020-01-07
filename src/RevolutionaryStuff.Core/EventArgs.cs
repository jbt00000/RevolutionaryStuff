using System;
using System.Diagnostics;

namespace RevolutionaryStuff.Core
{
    public class EventArgs<T> : EventArgs
    {
        /// <summary>
        /// The event's data
        /// </summary>
        public readonly T Data;

        [DebuggerStepThrough]
        public EventArgs(T data)
        {
            Data = data;
        }
    }
}
