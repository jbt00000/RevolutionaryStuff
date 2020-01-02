using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core
{
    public static class StreamHelpers
    {
        public static async Task CopyFromAsync(this Stream st, string path)
        {
            Requires.ReadableStreamArg(st, nameof(st));
            Requires.FileExists(path, nameof(path));

            using (var src = File.OpenRead(path))
            {
                await src.CopyToAsync(st);
            }
        }

        public static void CopyFrom(this Stream st, string path)
        {
            Requires.ReadableStreamArg(st, nameof(st));
            Requires.FileExists(path, nameof(path));

            using (var src = File.OpenRead(path))
            {
                src.CopyTo(st);
            }
        }

        public static void CopyTo(this Stream st, string path)
        {
            Requires.ReadableStreamArg(st, nameof(st));

            using (var dst = File.Create(path))
            {
                st.CopyTo(dst);
            }
        }

        public static async Task CopyToAsync(this Stream st, string path)
        {
            Requires.ReadableStreamArg(st, nameof(st));

            using (var dst = File.Create(path))
            {
                await st.CopyToAsync(dst);
            }
        }

        public static async Task CopyToAsync(this Stream st, Stream dst, Action<int, long, long?> progress, int? bufferSize=null)
        {
            Requires.ReadableStreamArg(st, nameof(st));
            Requires.WriteableStreamArg(dst, nameof(dst));

            var bufSize = bufferSize.GetValueOrDefault(1024 * 256);
            Requires.Positive(bufSize, nameof(bufferSize));

            var buf = new byte[bufSize];
            long tot = 0;
            long? len = 0;
            if (st.CanSeek)
            {
                len = st.Length - st.Position;
            }
            for (; ; )
            {
                int read = await st.ReadAsync(buf, 0, buf.Length);
                if (read <= 0) break;
                await dst.WriteAsync(buf, 0, read);
                tot += read;
                progress(read, tot, len);
            }
            progress(0, tot, len);
        }

        public static Stream Create(string s, Encoding e=null)
        {            
            var st = new MemoryStream();
            var sr = new StreamWriter(st, e ?? UTF8Encoding.UTF8);
            sr.Write(s);
            sr.Flush();
            st.Position = 0;
            return st;
        }

        public static void Write(this Stream st, byte[] buffer)
        {
            if (buffer == null) return;
            st.Write(buffer, 0, buffer.Length);
        }

        public static async Task<string> ReadToEndAsync(this Stream st, Encoding enc = null)
        {
            using (var sr = new StreamReader(st, enc ?? Encoding.UTF8))
            {
                return await sr.ReadToEndAsync();
            }
        }

        public static string ReadToEnd(this Stream st, Encoding enc=null)
        {
            using (var sr = new StreamReader(st, enc ?? Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Seek via the Position member
        /// </summary>
        /// <remarks>
        /// Used for implemeters of a stream
        /// </remarks>
        /// <param name="st">The stream</param>
        /// <param name="offset">The offset</param>
        /// <param name="origin">The origin</param>
        /// <returns>The new position</returns>
        public static long SeekViaPos(this Stream st, long offset, SeekOrigin origin)
        {
            long p;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    p = offset;
                    break;
                case SeekOrigin.Current:
                    p = st.Position + offset;
                    break;
                case SeekOrigin.End:
                    p = st.Length + offset;
                    break;
                default:
                    throw new UnexpectedSwitchValueException(origin);
            }
            st.Position = p;
            return p;
        }

        public static void ReadExactSize(this Stream st, byte[] buf, int offset = 0, int? size = null)
        {
            var remaining = size.GetValueOrDefault(buf.Length);
            while (remaining > 0)
            {
                int amtRead = st.Read(buf, offset, remaining);
                if (amtRead <= 0) break;
                remaining -= amtRead;
                offset += amtRead;
            }
            if (remaining > 0) throw new IndexOutOfRangeException($"{nameof(st)} was too small.  could not read remaining {remaining} bytes");
        }
    }
}
