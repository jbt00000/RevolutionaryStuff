using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.EncoderDecoders;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class BaseXTests
{
    [TestMethod]
    public void Base16DecodeNullStringTest()
        => Assert.IsTrue(CompareHelpers.Compare(Empty.ByteArray, Base16.Decode(null)));

    [TestMethod]
    public void Base16DecodeBlankStringTest()
        => Assert.IsTrue(CompareHelpers.Compare(Empty.ByteArray, Base16.Decode("     ")));

    [TestMethod]
    public void Base32DecodeNullStringTest()
        => Assert.IsTrue(CompareHelpers.Compare(Empty.ByteArray, Base32.Decode(null)));

    [TestMethod]
    public void Base32DecodeBlankStringTest()
        => Assert.IsTrue(CompareHelpers.Compare(Empty.ByteArray, Base32.Decode("     ")));

    [TestMethod]
    public void Base64DecodeNullStringTest()
        => Assert.IsTrue(CompareHelpers.Compare(Empty.ByteArray, Base64.Decode(null)));

    [TestMethod]
    public void Base64DecodeBlankStringTest()
        => Assert.IsTrue(CompareHelpers.Compare(Empty.ByteArray, Base64.Decode("     ")));

    [TestMethod]
    public void Base16EncodeDecodeTests()
    {
        var charset = new[] {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F' };
        for (var z = 0; z < 100; ++z)
        {
            var len = Stuff.RandomWithRandomSeed.Next(10) * 2 + 2;
            var s = "";
            for (var l = 0; l < len; ++l)
            {
                var n = Stuff.RandomWithRandomSeed.Next(charset.Length);
                s += charset[n];
            }
            var expected = Base16.Decode(s);
            var encoded = Base16.Encode(expected);
            Assert.AreEqual(s, encoded);
        }
    }
}
