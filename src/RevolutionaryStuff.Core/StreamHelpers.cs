using System.IO;
using System.Text;

namespace RevolutionaryStuff.Core
{
    public static class StreamHelpers
    {
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

        public static string ReadToEnd(this Stream st, Encoding enc=null)
        {
            if (null == enc)
            {
                enc = Encoding.UTF8;
            }
            using (var sr = new StreamReader(st, Encoding.UTF8))
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
    }
}
