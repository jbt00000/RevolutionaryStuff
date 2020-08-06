using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Streams
{
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

        public override long Length => Inner.Length;

        public override long Position { get => Inner.Position; set => Inner.Position = value; }

        public override void Flush()
        { 
            Inner.Flush();
            DirtyEvent.SafeInvoke(this);
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Inner.Read(buffer, offset, count);

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
            await base.FlushAsync(cancellationToken);
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
                await base.WriteAsync(buffer, offset, count, cancellationToken);
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
}
