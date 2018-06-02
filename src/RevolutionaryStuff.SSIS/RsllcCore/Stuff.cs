using System;
using System.Diagnostics;

namespace RevolutionaryStuff.Core
{
    public static class Stuff
    {
        public const string Qbert = "@!#?@!";

        /// <summary>
        /// Does nothing.  It is simply used as a line where one can set breakpoints
        /// </summary>
        /// <param name="args">Pass in parameters if you don't want them compiled out</param>
        [Conditional("DEBUG")]
        public static void Noop(params object[] args)
        {
        }

        /// <summary>
        /// Wed, 01 Oct 2008 17:04:32 GMT
        /// </summary>
        public static string ToRfc7231(this DateTime dt)
            => dt.ToUniversalTime().ToString("r");

        /// <summary>
        /// 2008-10-01T17:04:32.0000000Z
        /// </summary>
        public static string ToRfc8601(this DateTime dt)
            => dt.ToUniversalTime().ToString("o") + "Z";

        /// <summary>
        /// Dispose an object if it has an IDisposable interface
        /// </summary>
        /// <param name="o">The object</param>
        public static void Dispose(params object[] os)
        {
            if (os == null) return;
            foreach (object o in os)
            {
                var d = o as IDisposable;
                if (d == null) return;
                try
                {
                    d.Dispose();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

    }
}
