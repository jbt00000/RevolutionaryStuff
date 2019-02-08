using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class BasicCacherTests
    {
        [TestMethod]
        public async Task WorksAsync()
        {
            int maxCachedItemCount = 5;
            var basicCacher = new BasicCacher(maxCachedItemCount);
            var cacher = (ICacher)basicCacher;
            for (int z = 0; z < 10; ++z)
            {
                string cacheKey = Cache.CreateKey(z);
                var res = await cacher.FindEntryOrCreateValueAsync(
                    cacheKey,
                    k =>
                    {
                        Assert.AreEqual(cacheKey, k);
                        return Task.FromResult(new CacheCreationResult(z, new CacheEntryRetentionPolicy(TimeSpan.FromMinutes(10))));
                    });
                Assert.IsNotNull(res);
                Assert.IsFalse(res.IsExpired);
                Assert.AreEqual(z, res.GetValue<int>());
                Assert.IsTrue(basicCacher.CacheEntryByCacheKey.Count <= maxCachedItemCount);
            }
            var kvp = basicCacher.CacheEntryByCacheKey.First();
            Assert.AreEqual(kvp.Value.GetValue<int>(), cacher.FindOrCreateValue(kvp.Key, () => 99));
            Assert.AreEqual(100, cacher.FindOrCreateValue(kvp.Key, () => 100, null, true));
        }

        [TestMethod]
        public async Task RemoveTestAsync()
        {
            var basicCacher = new BasicCacher();
            var cacher = (ICacher)basicCacher;
            var cacheKey = "jason";
            var cacheVal = "thomas";
            var entry = await cacher.FindEntryOrCreateValueAsync(cacheKey, k => Task.FromResult(new CacheCreationResult(cacheVal)));
            Assert.AreEqual(cacheVal, entry.GetValue<string>());
            Assert.AreEqual(1, basicCacher.CacheEntryByCacheKey.Count);
            await cacher.RemoveAsync(cacheKey);
            Assert.AreEqual(0, basicCacher.CacheEntryByCacheKey.Count);
        }

        [TestMethod]
        public async Task ExpiredTestAsync()
        {
            var expirationTimeout = TimeSpan.FromSeconds(3);
            var basicCacher = new BasicCacher();
            var cacher = (ICacher)basicCacher;
            var cacheKey = "jason";
            var cacheVal = "thomas";
            var retVal = cacher.FindOrCreateValue(cacheKey, () => cacheVal, expirationTimeout, false);
            Assert.AreEqual(cacheVal, retVal);
            var entry = await cacher.FindEntryOrCreateValueAsync(cacheKey);
            Assert.IsNotNull(entry);
            Assert.AreEqual(cacheVal, entry.GetValue<string>());
            Thread.Sleep(expirationTimeout);
            entry = await cacher.FindEntryOrCreateValueAsync(cacheKey);
            Assert.IsNull(entry);
        }
    }
}
