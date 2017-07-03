using System.Collections.Generic;

namespace RevolutionaryStuff.Core
{
    public static class CompareHelpers
    {
        /// <summary>
        /// Compare 2 byte arrays to see if their enclosed data is the same
        /// </summary>
        /// <param name="b1">ByteArray#1</param>
        /// <param name="b2">ByteArray#2</param>
        /// <returns>True if their data is the same, else false</returns>
        public static bool Compare(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null || b1.Length != b2.Length) return false;
            int len = b1.Length;
            for (int x = 0; x < len; ++x)
            {
                if (b1[x] != b2[x])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Contains<T>(this T[] items, T test) => ((ICollection<T>)items).Contains(test);
    }
}
