using System.IO;

namespace RevolutionaryStuff.Core.Streams
{
    public class IndestructibleStream : Stream
    {
        public IndestructibleStream(Stream inner)
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
            => Inner.Flush();

        public override int Read(byte[] buffer, int offset, int count)
            => Inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin)
            => Inner.Seek(offset, origin);

        public override void SetLength(long value)
            => Inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
            => Inner.Write(buffer, offset, count);

        public override void Close()
        { }
    }
}
