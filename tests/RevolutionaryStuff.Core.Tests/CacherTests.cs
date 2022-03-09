using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        [TestMethod]
        public async Task FindOrCreateTwiceShouldOnlyCreateOnceAsync()
        {
            const string expectedVal = "this is my cached value";
            var cacher = new BasicCacher(16);
            var res = await cacher.FindOrCreateValueAsync(
                nameof(FindOrCreateTwiceShouldOnlyCreateOnceAsync),
                () => Task.FromResult(expectedVal));
            Assert.AreEqual(expectedVal, res);
            bool fresh = false;
            res = await cacher.FindOrCreateValueAsync(
                nameof(FindOrCreateTwiceShouldOnlyCreateOnceAsync),
                () =>
                {
                    fresh = true;
                    return Task.FromResult(expectedVal);
                });
            Assert.AreEqual(expectedVal, res);
            Assert.AreEqual(false, fresh);
        }

        [TestMethod]
        public void DontRemainLockedOnException()
        {
            var cacher = new BasicCacher(16);

            var key = nameof(DontRemainLockedOnException);
            const int val = 1232;

            try
            {
                cacher.FindOrCreateValue<int>(key, () => throw new Exception("bbsdsf"));
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception)
            { }

            var actual = cacher.FindOrCreateValue(key, () => val);

            Assert.AreEqual(val, actual);
        }

        [TestMethod]
        public async Task OtherCallersWaitAsync()
        {
            var cacher = new BasicCacher(16);
            var key1 = "one";
            int calls1 = 0;
            var key2 = "two";
            int calls2 = 0;

            var callWait = TimeSpan.FromSeconds(2);

            Stopwatch sw = new();
            sw.Start();

            await Task.WhenAll(Enumerable.Range(1, 100).Select(z =>
                cacher.FindOrCreateValueAsync(
                    z % 2 == 0 ? key1 : key2,
                    async () =>
                    {
                        await Task.Delay(callWait);
                        if (z % 2 == 0)
                        {
                            return Interlocked.Increment(ref calls1);
                        }
                        else
                        {
                            return Interlocked.Increment(ref calls2);
                        }
                    }
                    )));

            sw.Stop();

            Assert.AreEqual(1, calls1);
            Assert.AreEqual(1, calls2);
            Assert.IsTrue(sw.Elapsed < callWait * 2);
        }
    }
}
