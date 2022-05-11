using System.Diagnostics;
using System.IO;
using System.Text;
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
            var callbackCount = 0;
            long lastTotRead = 0;
            await sourceStream.CopyToAsync(destStream, (read, totRead, tot) => 
            {
                ++callbackCount;
                Trace.WriteLine($"CopyToAsync(read={read}, totRead={totRead}, tot={tot}) CallbackCount={callbackCount}");
                Assert.IsTrue(totRead >= lastTotRead);
                Assert.AreEqual(totRead, lastTotRead+read);
                lastTotRead = totRead;
            });
            Assert.IsTrue(callbackCount > 1);
            Assert.IsTrue(CompareHelpers.Compare(sourceBuffer, destStream.ToArray()));
        }

        [TestMethod]
        public async Task ReadToEndAsyncTest()
        {
            var test = $"{nameof(ReadToEndAsyncTest)} message.";
            var st = new MemoryStream();
            var sw = new StreamWriter(st, Encoding.UTF8);
            sw.Write(test);
            sw.Flush();
            st.Position = 0;
            var ans = await st.ReadToEndAsync();
            Assert.AreEqual(test, ans);
            //the below will throw an exception if readtoend closed the stream
            st.Position = 0;
        }

        [TestMethod]
        public void ReadToEndTest()
        {
            var test = $"{nameof(ReadToEndTest)} message.";
            var st = new MemoryStream();
            var sw = new StreamWriter(st, Encoding.UTF8);
            sw.Write(test);
            sw.Flush();
            st.Position = 0;
            var ans = st.ReadToEnd();
            Assert.AreEqual(test, ans);
            //the below will throw an exception if readtoend closed the stream
            st.Position = 0;
        }
    }
}
