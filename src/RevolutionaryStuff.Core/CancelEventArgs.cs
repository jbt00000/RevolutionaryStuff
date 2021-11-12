using System.ComponentModel;
using System.Diagnostics;

namespace RevolutionaryStuff.Core;

public class CancelEventArgs<T> : CancelEventArgs
{
    public readonly T Data;

    [DebuggerStepThrough]
    public CancelEventArgs(T data) => this.Data = data;
}
