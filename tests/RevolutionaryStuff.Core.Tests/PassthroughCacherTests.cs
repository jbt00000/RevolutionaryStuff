using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class PassthroughCacherTests
    {
        [TestMethod]
        public async Task WorksAsync()
        {
            var cacher = (ICacher) Cache.Passthrough;
            for (int z = 0; z < 10; ++z)
            {
                string cacheKey = Cache.CreateKey(z);
                var res = await cacher.FindEntryOrCreateValueAsync(
                    cacheKey,
                    k => {
                        Assert.AreEqual(cacheKey, k);
                        return Task.FromResult(new CacheCreationResult(z, new CacheEntryRetentionPolicy(TimeSpan.FromMinutes(10))));
                    });
                Assert.IsNotNull(res);
                Assert.IsFalse(res.IsExpired);
                Assert.AreEqual(z, res.GetValue<int>());
            }
        }
    }
}
