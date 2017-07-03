using System.Diagnostics;

namespace RevolutionaryStuff.Core
{
    public static class Stuff
    {
        [Conditional("DEBUG")]
        public static void Noop(params object[] args)
        {
        }
    }
}
