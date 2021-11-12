using System.IO;

namespace RevolutionaryStuff.Core.Streams;

public class IndestructibleStream : MonitoredStream
{
    public bool PreventClose { get; private set; }

    public IndestructibleStream(Stream inner, bool preventClose = true)
        : base(inner)
    {
        PreventClose = preventClose;
        CloseEvent += (s, a) => a.Cancel = PreventClose;
        DisposeEvent += (s, a) => a.Cancel = PreventClose;
    }
}
