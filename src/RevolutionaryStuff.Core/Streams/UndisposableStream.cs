using System;
using System.IO;

namespace RevolutionaryStuff.Core.Streams
{
    public class IndestructibleStream : Stream
    {
        public bool PreventClose { get; private set; }
        public event EventHandler DirtyEvent;
        public event EventHandler CloseEvent;

        public IndestructibleStream(Stream inner, bool preventClose=true)
        {
            Inner = inner;
            PreventClose = preventClose;
        }

        public Stream Inner { get; }

        public override bool CanRead => Inner.CanRead;

        public override bool CanSeek => Inner.CanSeek;

        public override bool CanWrite => Inner.CanWrite;

        public override long Length => Inner.Length;

        public override long Position { get => Inner.Position; set => Inner.Position = value; }

        public override void Flush()
            => Inner.Flush();

        public override int Read(byte[] buffer, int offset, int count)
            => Inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin)
            => Inner.Seek(offset, origin);

        public override void SetLength(long value)
        { 
            Inner.SetLength(value);
            DelegateHelpers.SafeInvoke(DirtyEvent, this, EventArgs.Empty, false);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Inner.Write(buffer, offset, count);
            DelegateHelpers.SafeInvoke(DirtyEvent, this, EventArgs.Empty, false);
        }

        public override void Close()
        {
            base.Close();
            DelegateHelpers.SafeInvoke(CloseEvent, this, EventArgs.Empty, false);
            if (!PreventClose)
            {
                Inner.Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DelegateHelpers.SafeInvoke(CloseEvent, this, EventArgs.Empty, false);
            if (!PreventClose)
            {
                Inner.Dispose();
            }
        }
    }
}
