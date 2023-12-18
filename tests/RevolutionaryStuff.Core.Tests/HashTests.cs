using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Crypto;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class HashTests
{
    [TestMethod]
    public void Crc32Tests()
    {
        const int iterations = 100;
        var sw = new Stopwatch();
        for (int z = 0; z < 32; ++z)
        {
            var buf = new byte[Stuff.RandomWithFixedSeed.Next(1024*8)+128];
            foreach (var hashAlgName in Hash.CommonHashAlgorithmNames.All)
            {
                try
                {
                    Hash h = null;
                    sw.Restart();
                    for (int i = 0; i < iterations; ++i)
                    {
                        h = Hash.Compute(buf, hashAlgName);
                    }
                    sw.Stop();
                    Debug.WriteLine($"{h.NameColonBase16} took {sw.Elapsed/iterations}");
                }
                catch (NotSupportedException)
                {
                    Debug.WriteLine($"{hashAlgName} is not supported");
                }
            }
        }
    }
}
