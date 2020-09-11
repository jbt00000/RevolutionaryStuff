using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class JsonNetTests
    {
        [TestMethod]
        public void SetValueRootTest()
        {
            var j = JObject.Parse("{}");
            Assert.AreEqual(0, j.Count);
            j.SetValue("a", 1);
            Assert.AreEqual(1, j["a"].Value<int>());
            Assert.AreEqual(1, j.Count);
            j.SetValue("b", 2);
            Assert.AreEqual(2, j["b"].Value<int>());
            Assert.AreEqual(2, j.Count);
        }

        [TestMethod]
        public void OverwriteValueRootTest()
        {
            var j = JObject.Parse("{}");
            j.SetValue("a", 1);
            Assert.AreEqual(1, j["a"].Value<int>());
            j.SetValue("a", "aa");
            Assert.AreEqual("aa", j["a"].Value<string>());
            Assert.AreEqual(1, j.Count);
        }

        [TestMethod]
        public void SetChildObjectTest()
        {
            var j = JObject.Parse("{}");
            j.SetValue("a.b", "ab");
            Assert.AreEqual("ab", j["a"]["b"].Value<string>());
            Assert.AreEqual(1, j.Count);
            Assert.AreEqual(1, ((JContainer)j["a"]).Count);
        }
    }
}
