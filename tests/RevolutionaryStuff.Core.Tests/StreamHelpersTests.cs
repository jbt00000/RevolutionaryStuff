using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class StreamHelpersTests
    {
        [TestMethod]
        public async Task CopyToAsyncTest()
        {
            var sourceBuffer = new byte[1024 * 1024 * 4];
            Stuff.Random.NextBytes(sourceBuffer);
            var sourceStream = new MemoryStream(sourceBuffer);
            var destStream = new MemoryStream();
            int callbackCount = 0;
            long lastTotRead = 0;
            await sourceStream.CopyToAsync(destStream, (read, totRead, tot) => 
            {
                ++callbackCount;
                Assert.IsTrue(totRead >= lastTotRead);
                Assert.AreEqual(totRead, lastTotRead+read);
                lastTotRead = totRead;
            });
            Assert.IsTrue(callbackCount > 1);
            Assert.IsTrue(CompareHelpers.Compare(sourceBuffer, destStream.ToArray()));
        }
    }
}
