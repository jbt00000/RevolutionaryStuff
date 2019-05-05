using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class CacherTests
    {
        [TestMethod]
        public void CreateKeyTests()
        {
            var r0 = Cache.CreateKey<int>();
            var r1 = Cache.CreateKey<int>(5);
            var r2 = Cache.CreateKey<int>(1, 2);
            var r3 = Cache.CreateKey<int>(1, 2, "a");
            var r4 = Cache.CreateKey("hello", nameof(CreateKeyTests));
            for (int z = 0; z < 5; ++z)
            {
                Assert.AreEqual(r0, Cache.CreateKey<int>());
                Assert.AreEqual(r1, Cache.CreateKey<int>(5));
                Assert.AreNotEqual(r1, Cache.CreateKey<int>(6));
                Assert.AreEqual(r2, Cache.CreateKey<int>(1, 2));
                Assert.AreNotEqual(r2, Cache.CreateKey<int>(1, 3));
                Assert.AreEqual(r3, Cache.CreateKey<int>(1, 2, "a"));
                Assert.AreNotEqual(r3, Cache.CreateKey<int>(1, 2, "b"));
                Assert.AreEqual(r4, Cache.CreateKey("hello", nameof(CreateKeyTests)));
                Assert.AreNotEqual(r4, Cache.CreateKey("helloworld", nameof(CreateKeyTests)));
            }
        }
    }
}
