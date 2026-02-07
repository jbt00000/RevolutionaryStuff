using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Crypto;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class HashTests
{
    private const string TestData = "Hello, World!";
    private static readonly byte[] TestDataBytes = Encoding.UTF8.GetBytes(TestData);

    #region Hash Algorithm Registration Tests

    [TestMethod]
    public void Crc32_IsRegistered()
    {
        Assert.IsTrue(Hash.IsHashAlgorithmInstalled(Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc32));
    }

    [TestMethod]
    public void XxHash32_IsRegistered()
    {
        Assert.IsTrue(Hash.IsHashAlgorithmInstalled(Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32));
    }

    [TestMethod]
    public void XxHash64_IsRegistered()
    {
        Assert.IsTrue(Hash.IsHashAlgorithmInstalled(Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64));
    }

    [TestMethod]
    public void XxHash128_IsRegistered()
    {
        Assert.IsTrue(Hash.IsHashAlgorithmInstalled(Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash128));
    }

    [TestMethod]
    public void AllNonCryptographicAlgorithms_AreRegistered()
    {
        foreach (var algorithmName in Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.All)
        {
            Assert.IsTrue(Hash.IsHashAlgorithmInstalled(algorithmName), $"Algorithm '{algorithmName}' should be registered");
        }
    }

    #endregion

    #region CreateHashAlgorithm Tests

    [TestMethod]
    public void CreateHashAlgorithm_Crc32_ReturnsValidHasher()
    {
        var hasher = Hash.CreateHashAlgorithm(Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc32);
        Assert.IsNotNull(hasher);
    }

    [TestMethod]
    public void CreateHashAlgorithm_XxHash32_ReturnsValidHasher()
    {
        var hasher = Hash.CreateHashAlgorithm(Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32);
        Assert.IsNotNull(hasher);
    }

    [TestMethod]
    public void CreateHashAlgorithm_XxHash64_ReturnsValidHasher()
    {
        var hasher = Hash.CreateHashAlgorithm(Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);
        Assert.IsNotNull(hasher);
    }

    [TestMethod]
    public void CreateHashAlgorithm_XxHash128_ReturnsValidHasher()
    {
        var hasher = Hash.CreateHashAlgorithm(Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash128);
        Assert.IsNotNull(hasher);
    }

    [TestMethod]
    public void CreateHashAlgorithm_UnknownAlgorithm_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() => Hash.CreateHashAlgorithm("unknown-algorithm"));
    }

    #endregion

    #region Stream Hashing Tests (Append and GetHashAndReset)

    [TestMethod]
    public void Compute_WithStream_Crc32_ReturnsValidHash()
    {
        using var stream = new MemoryStream(TestDataBytes);
        var hash = Hash.Compute(stream, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc32);

        Assert.IsNotNull(hash);
        Assert.IsNotNull(hash.Data);
        Assert.IsTrue(hash.Data.Length > 0);
        Assert.AreEqual(Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc32, hash.HashName);
    }

    [TestMethod]
    public void Compute_WithStream_XxHash32_ReturnsValidHash()
    {
        using var stream = new MemoryStream(TestDataBytes);
        var hash = Hash.Compute(stream, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32);

        Assert.IsNotNull(hash);
        Assert.IsNotNull(hash.Data);
        Assert.AreEqual(4, hash.Data.Length); // XxHash32 produces 4 bytes
    }

    [TestMethod]
    public void Compute_WithStream_XxHash64_ReturnsValidHash()
    {
        using var stream = new MemoryStream(TestDataBytes);
        var hash = Hash.Compute(stream, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);

        Assert.IsNotNull(hash);
        Assert.IsNotNull(hash.Data);
        Assert.AreEqual(8, hash.Data.Length); // XxHash64 produces 8 bytes
    }

    [TestMethod]
    public void Compute_WithStream_XxHash128_ReturnsValidHash()
    {
        using var stream = new MemoryStream(TestDataBytes);
        var hash = Hash.Compute(stream, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash128);

        Assert.IsNotNull(hash);
        Assert.IsNotNull(hash.Data);
        Assert.AreEqual(16, hash.Data.Length); // XxHash128 produces 16 bytes
    }

    [TestMethod]
    public void Compute_WithByteArray_Crc32_ReturnsValidHash()
    {
        var hash = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc32);

        Assert.IsNotNull(hash);
        Assert.IsNotNull(hash.Data);
        Assert.IsTrue(hash.Data.Length > 0);
    }

    [TestMethod]
    public void Compute_WithByteArray_XxHash32_ReturnsValidHash()
    {
        var hash = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32);

        Assert.IsNotNull(hash);
        Assert.IsNotNull(hash.Data);
        Assert.AreEqual(4, hash.Data.Length);
    }

    [TestMethod]
    public void Compute_WithByteArray_XxHash64_ReturnsValidHash()
    {
        var hash = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);

        Assert.IsNotNull(hash);
        Assert.IsNotNull(hash.Data);
        Assert.AreEqual(8, hash.Data.Length);
    }

    [TestMethod]
    public void Compute_WithByteArray_XxHash128_ReturnsValidHash()
    {
        var hash = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash128);

        Assert.IsNotNull(hash);
        Assert.IsNotNull(hash.Data);
        Assert.AreEqual(16, hash.Data.Length);
    }

    #endregion

    #region Hash Consistency Tests

    [TestMethod]
    public void Compute_SameInput_ProducesSameHash_Crc32()
    {
        var hash1 = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc32);
        var hash2 = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc32);

        Assert.AreEqual(hash1.Urn, hash2.Urn);
        CollectionAssert.AreEqual(hash1.Data, hash2.Data);
    }

    [TestMethod]
    public void Compute_SameInput_ProducesSameHash_XxHash64()
    {
        var hash1 = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);
        var hash2 = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);

        Assert.AreEqual(hash1.Urn, hash2.Urn);
        CollectionAssert.AreEqual(hash1.Data, hash2.Data);
    }

    [TestMethod]
    public void Compute_DifferentInput_ProducesDifferentHash()
    {
        var hash1 = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);
        var hash2 = Hash.Compute(Encoding.UTF8.GetBytes("Different Data"), Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);

        Assert.AreNotEqual(hash1.Urn, hash2.Urn);
        CollectionAssert.AreNotEqual(hash1.Data, hash2.Data);
    }

    [TestMethod]
    public void Compute_StreamAndByteArray_ProduceSameHash()
    {
        using var stream = new MemoryStream(TestDataBytes);
        var hashFromStream = Hash.Compute(stream, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);
        var hashFromBytes = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);

        Assert.AreEqual(hashFromStream.Urn, hashFromBytes.Urn);
        CollectionAssert.AreEqual(hashFromStream.Data, hashFromBytes.Data);
    }

    #endregion

    #region Hash URN and Equality Tests

    [TestMethod]
    public void Hash_Urn_HasCorrectFormat_Crc32()
    {
        var hash = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc32);

        Assert.IsTrue(hash.Urn.StartsWith("urn:crc32:"));
    }

    [TestMethod]
    public void Hash_Urn_HasCorrectFormat_XxHash32()
    {
        var hash = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32);

        Assert.IsTrue(hash.Urn.StartsWith("urn:xxh32:"));
    }

    [TestMethod]
    public void Hash_Urn_HasCorrectFormat_XxHash64()
    {
        var hash = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);

        Assert.IsTrue(hash.Urn.StartsWith("urn:xxh64:"));
    }

    [TestMethod]
    public void Hash_Urn_HasCorrectFormat_XxHash128()
    {
        var hash = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash128);

        Assert.IsTrue(hash.Urn.StartsWith("urn:xxh128:"));
    }

    [TestMethod]
    public void Hash_Equality_SameHash_ReturnsTrue()
    {
        var hash1 = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32);
        var hash2 = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32);

        Assert.AreEqual(hash1, hash2);
        Assert.IsTrue(hash1 == hash2);
    }

    [TestMethod]
    public void Hash_Equality_DifferentHash_ReturnsFalse()
    {
        var hash1 = Hash.Compute(TestDataBytes, Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32);
        var hash2 = Hash.Compute(Encoding.UTF8.GetBytes("Other"), Hash.CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32);

        Assert.AreNotEqual(hash1, hash2);
        Assert.IsTrue(hash1 != hash2);
    }

    #endregion

    #region Original Performance Test

    [TestMethod]
    public void AllHashAlgorithms_PerformanceTest()
    {
        const int iterations = 100;
        var sw = new Stopwatch();
        for (var z = 0; z < 32; ++z)
        {
            var buf = new byte[Stuff.RandomWithFixedSeed.Next(1024 * 8) + 128];
            foreach (var hashAlgName in Hash.CommonHashAlgorithmNames.All)
            {
                try
                {
                    Hash h = null;
                    sw.Restart();
                    for (var i = 0; i < iterations; ++i)
                    {
                        h = Hash.Compute(buf, hashAlgName);
                    }
                    sw.Stop();
                    Debug.WriteLine($"{h.NameColonBase16} took {sw.Elapsed / iterations}");
                }
                catch (NotSupportedException)
                {
                    Debug.WriteLine($"{hashAlgName} is not supported");
                }
            }
        }
    }

    #endregion
}
