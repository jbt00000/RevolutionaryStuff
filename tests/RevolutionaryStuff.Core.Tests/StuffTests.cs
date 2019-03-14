using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class StuffTests
    {
        [TestMethod]
        public void SwapTests()
        {
            int a = 1, b = 2;
            Stuff.Swap(ref a, ref b);
            Assert.AreEqual(2, a);
            Assert.AreEqual(1, b);
        }

        [TestMethod]
        public void CoalesceStringsTests()
        {
            Assert.IsNull(Stuff.CoalesceStrings());
            Assert.IsNull(Stuff.CoalesceStrings(""));
            Assert.AreEqual("a", Stuff.CoalesceStrings("a"));
            Assert.AreEqual("a", Stuff.CoalesceStrings("a", "b"));
            Assert.AreEqual("b", Stuff.CoalesceStrings(null, "b"));
            Assert.AreEqual("b", Stuff.CoalesceStrings("", "b"));
        }

        [TestMethod]
        public void MinTests()
        {
            Assert.AreEqual(5, Stuff.Min(12, 5));
            Assert.AreEqual(5, Stuff.Min(5, 12));
        }

        [TestMethod]
        public void MaxTests()
        {
            Assert.AreEqual(12, Stuff.Max(12, 5));
            Assert.AreEqual(12, Stuff.Max(5, 12));
        }
    }
}
