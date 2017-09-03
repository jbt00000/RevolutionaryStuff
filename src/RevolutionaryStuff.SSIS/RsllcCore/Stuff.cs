using System;
using System.Diagnostics;

namespace RevolutionaryStuff.Core
{
    public static class Stuff
    {
        /// <summary>
        /// Does nothing.  It is simply used as a line where one can set breakpoints
        /// </summary>
        /// <param name="args">Pass in parameters if you don't want them compiled out</param>
        [Conditional("DEBUG")]
        public static void Noop(params object[] args)
        {
        }

        public static string ToRfc7231(this DateTime dt)
            => dt.ToUniversalTime().ToString("r");

    }
}
