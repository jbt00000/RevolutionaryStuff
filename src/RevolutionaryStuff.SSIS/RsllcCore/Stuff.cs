using System.Diagnostics;

namespace RevolutionaryStuff.SSIS.RsllcCore
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
    }
}
