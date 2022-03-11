using System.ComponentModel;
using System.IO;
using System.Threading;

namespace RevolutionaryStuff.Core.Streams;

public class MonitoredStream : Stream
{
    public event EventHandler DirtyEvent;
    public event CancelEventHandler CloseEvent;
    public event CancelEventHandler DisposeEvent;
    public event EventHandler<EventArgs<long>> NewLengthEvent;

    public MonitoredStream(Stream inner)
    {
        Inner = inner;
    }

    public Stream Inner { get; }

    public override bool CanRead => Inner.CanRead;

    public override bool CanSeek => Inner.CanSeek;

    public override bool CanWrite => Inner.CanWrite;

    public override int ReadTimeout { get => Inner.ReadTimeout; set => Inner.ReadTimeout = value; }

    public override int WriteTimeout { get => Inner.WriteTimeout; set => Inner.WriteTimeout = value; }

    public override long Length => Inner.Length;

    public override long Position { get => Inner.Position; set => Inner.Position = value; }

    public override bool CanTimeout => Inner.CanTimeout;

    public override void Flush()
    {
        Inner.Flush();
        DirtyEvent.SafeInvoke(this);
    }

    public override int Read(byte[] buffer, int offset, int count)
        => Inner.Read(buffer, offset, count);

    public override int ReadByte()
        => Inner.ReadByte();

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => Inner.ReadAsync(buffer, offset, count, cancellationToken);

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        => Inner.BeginRead(buffer, offset, count, callback, state);

    public override int EndRead(IAsyncResult asyncResult)
        => Inner.EndRead(asyncResult);

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        => Inner.CopyToAsync(destination, bufferSize, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin)
        => Inner.Seek(offset, origin);

    public override void SetLength(long value)
    {
        NewLengthEvent.SafeInvoke(this, new EventArgs<long>(value));
        Inner.SetLength(value);
        DirtyEvent.SafeInvoke(this);
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        await Inner.FlushAsync(cancellationToken);
        DirtyEvent.SafeInvoke(this);
    }

    public override void WriteByte(byte value)
        => Write(new[] { value }, 0, 1);

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        NewLengthEvent.SafeInvoke(this, new EventArgs<long>(Inner.Position + count));
        return Inner.BeginWrite(buffer, offset, count, callback, state);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
        Inner.EndWrite(asyncResult);
        DirtyEvent.SafeInvoke(this);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (count > 0)
        {
            NewLengthEvent.SafeInvoke(this, new EventArgs<long>(Inner.Position + count));
            Inner.Write(buffer, offset, count);
            DirtyEvent.SafeInvoke(this);
        }
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (count > 0)
        {
            NewLengthEvent.SafeInvoke(this, new EventArgs<long>(Inner.Position + count));
            await Inner.WriteAsync(buffer, offset, count, cancellationToken);
            DirtyEvent.SafeInvoke(this);
        }
    }

    public override void Close()
    {
        if (!CloseEvent.SafeInvoke(this))
        {
            Inner.Close();
            base.Close();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!DisposeEvent.SafeInvoke(this))
        {
            Inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
