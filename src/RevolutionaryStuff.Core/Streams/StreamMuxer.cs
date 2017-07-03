using System;
using System.Diagnostics;
using System.IO;

namespace RevolutionaryStuff.Core.Streams
{
    /// <summary>
    /// Allows a stream to be sub-divided into independent logical streams with varying access rights
    /// </summary>
    public class StreamMuxer : BaseDisposable
    {
        private readonly Stream Inner_p;

        /// <summary>
        /// When true, we should leave the inner stream open when the muxer
        /// is either closed or disposed.
        /// </summary>
        private readonly bool LeaveOpen;

        #region Constructors

        public StreamMuxer(Stream inner, bool leaveOpen=false)
        {
            Requires.StreamArg(inner, nameof(inner), false, false, true);
            Inner_p = inner;
            LeaveOpen = leaveOpen;
        }

        protected override void OnDispose(bool disposing)
        {
            base.OnDispose(disposing);
            if (!LeaveOpen)
            {
                Inner_p.Dispose();
            }
        }

        #endregion

        #region Quickies...

        /// <summary>
        /// The length of the underlying stream
        /// </summary>
        public long Length
        {
            get
            {
                lock (Inner)
                {
                    return Inner.Length;
                }
            }
        }

        /// <summary>
        /// Flush the contents of the underlying stream
        /// </summary>
        public void Flush()
        {
            lock (Inner)
            {
                Inner.Flush();
            }
        }

        /// <summary>
        /// Write to a section of the underlying stream
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <param name="offsetInFile">The place in the current stream to where we should begin to write data</param>
        public void Write(byte[] buffer, int offset, int count, long offsetInFile)
        {
            Requires.ArrayArg(buffer, offset, count, nameof(buffer));
            if (!Inner.CanWrite) throw new NotSupportedException();
            lock (Inner)
            {
                Inner.Position = offsetInFile;
                Inner.Write(buffer, offset, count);
            }
        }

        #endregion

        #region Sub-Stream Creation

        /// <summary>
        /// Create a new stream that is a subset of the underlying stream with the specified access rights
        /// </summary>
        /// <param name="canRead">When true, the new stream has read access</param>
        /// <param name="canWrite">When true, the new stream has write access</param>
        /// <param name="offset">The offset into the original stream to use as the base for this new stream</param>
        /// <param name="size">The size of this new stream, -1 if it should be adjusted according to the size of the underlying stream</param>
        /// <returns>A Stream</returns>
        public Stream Create(bool canRead=true, bool canWrite=true, long offset=0, long size=-1)
        {
            return new MyStream(this, canRead, canWrite, offset, size);
        }

        public Stream OpenRead() => Create(true, false, 0, -1);

        #endregion

        /// <summary>
        /// The underlying stream
        /// </summary>
        /// <remarks>
        /// While tempting to make this public, doing so is dangerous
        /// as the outside world could easily access the member's without 
        /// using the appropriate locks, which would kill us in multithreaded
        /// situations
        /// </remarks>
        protected Stream Inner
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(StreamMuxer));
                return Inner_p;
            }
        }

        #region Nested type: MyStream

        /// <summary>
        /// Our sub-stream
        /// </summary>
        private class MyStream : Stream
        {
            /// <summary>
            /// The muxer's stream
            /// </summary>
            private readonly Stream Inner;

            /// <summary>
            /// The parent muxer
            /// </summary>
            private readonly StreamMuxer Muxer;

            /// <summary>
            /// The offset into the parent "Inner" stream which serves as our base
            /// </summary>
            private readonly long Offset;

            /// <summary>
            /// The size of this stream.  When -1, this is the determined by the underlying stream
            /// </summary>
            private readonly long Size;

            /// <summary>
            /// Have we been closed
            /// </summary>
            private bool IsClosed;

            #region Constructors

            /// <summary>
            /// Construct a new stream
            /// </summary>
            /// <param name="muxer">The muxer</param>
            /// <param name="canRead">When true, the caller can read from this new stream</param>
            /// <param name="canWrite">When true, the caller can write to this new stream</param>
            /// <param name="offset">The offset into the parent stream to use as a base</param>
            /// <param name="size">The size of this new stream, -1 if determined by the parent</param>
            public MyStream(StreamMuxer muxer, bool canRead, bool canWrite, long offset, long size)
            {
                Requires.NonNull(muxer, nameof(muxer));

                Muxer = muxer;
                Inner = muxer.Inner;
                Requires.Between(offset, nameof(offset), 0, Inner.Length);
                if (size != -1)
                {
                    Requires.Between(size, nameof(size), 0, Inner.Length - offset + 1);
                }
                Offset = offset;
                Size = size;
                CanRead_p = canRead;
                CanWrite_p = canWrite;
            }

            #endregion

            #region Stream Overrides

            private readonly bool CanRead_p;

            private readonly bool CanWrite_p;
            private long Position_p;

            public override bool CanRead
            {
                [DebuggerStepThrough]
                get { return !IsClosed && CanRead_p && Muxer.Inner.CanRead; }
            }

            public override bool CanWrite
            {
                [DebuggerStepThrough]
                get { return !IsClosed && CanWrite_p && Muxer.Inner.CanWrite; }
            }

            public override bool CanSeek
            {
                [DebuggerStepThrough]
                get { return !IsClosed; }
            }

            public override long Length
            {
                get
                {
                    if (IsClosed) throw new NotNowException();
                    if (Size == -1)
                    {
                        return Inner.Length - Offset;
                    }
                    return Size;
                }
            }

            public override long Position
            {
                get
                {
                    if (IsClosed) throw new NotNowException();
                    return Position_p;
                }
                set
                {
                    if (IsClosed) throw new NotNowException();
                    try
                    {
                        Requires.Between(value, nameof(value), 0, Length);
                    }
                    catch (Exception ex)
                    {
                        //we rethrow so we can support the accepted Stream exception conventions
                        throw new NotSupportedException("New Position is past the acceptable bounds", ex);
                    }
                    Position_p = value;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                IsClosed = true;
            }

            public override long Seek(long offset, SeekOrigin origin) => this.SeekViaPos(offset, origin);

            public override void SetLength(long value)
            {
                if (!CanWrite) throw new InvalidOperationException();
                if (Size != -1 || Offset != 0) throw new InvalidOperationException();
                Inner.SetLength(value);
            }

            public override void Flush()
            {
                if (IsClosed) throw new NotNowException();
                Muxer.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (!CanRead) throw new NotNowException();
                Requires.ArrayArg(buffer, offset, count, nameof(buffer));
                count = (int) Math.Min(count, Length - Position);
                lock (Inner)
                {
                    Inner.Position = Offset + Position;
                    int read = Inner.Read(buffer, offset, count);
                    Position += read;
                    return read;
                }
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (!CanWrite) throw new ReadOnlyException();
                Muxer.Write(buffer, offset, count, Position);
                Position += count;
            }

            #endregion
        }

        #endregion
    }
}