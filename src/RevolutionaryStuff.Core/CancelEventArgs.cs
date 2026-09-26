using System.ComponentModel;
using System.Diagnostics;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides cancelable event arguments that carry an associated value.
/// </summary>
/// <typeparam name="T">The type of the associated value.</typeparam>
public class CancelEventArgs<T> : CancelEventArgs
{
    /// <summary>
    /// Gets the value associated with this event.
    /// </summary>
    public readonly T Data;

    /// <summary>
    /// Initializes a new instance with the specified associated value.
    /// </summary>
    /// <param name="data">The value associated with this event.</param>
    [DebuggerStepThrough]
    public CancelEventArgs(T data) => Data = data;
}
