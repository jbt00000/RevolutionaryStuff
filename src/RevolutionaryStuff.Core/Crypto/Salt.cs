using System;
using System.Security.Cryptography;

namespace RevolutionaryStuff.Core.Crypto
{
    public static class Salt
    {
        private static readonly RandomNumberGenerator RNG = RandomNumberGenerator.Create();

        public static void GetBytes(byte[] buf)
        {
            RNG.GetBytes(buf);
        }

        /// <summary>
        /// Return a random integer
        /// </summary>
        public static int RandomInteger
        {
            get
            {
                var data = new Byte[4];
                GetBytes(data);
                return BitConverter.ToInt32(data, 0);
            }
        }

        /// <summary>
        /// Create a 16 byte random buffer, and call it a guid
        /// </summary>
        /// <returns>A fresh non machine identifable guid</returns>
        public static Guid CreateGuid()
        {
            return new Guid(CreateRandomBuf(16));
        }

        /// <summary>
        /// Creates a salt with the specified length
        /// </summary>
        /// <param name="Length">The length of the salt</param>
        /// <returns>A buffer containing the salt</returns>
        public static byte[] CreateRandomBuf(uint Length)
        {
            var buf = new byte[Length];
            GetBytes(buf);
            return buf;
        }
    }
}
