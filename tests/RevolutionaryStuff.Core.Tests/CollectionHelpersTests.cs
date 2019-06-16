using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class CollectionHelpersTests
    {
        private static IList<int> CreateInOrderList(int elementCount = 100)
            => Enumerable.Range(0, elementCount).ToList();

        private void ValidateAllElementsExactlyOnce(IList<int> l, int start=0, int count=100)
        {
            var d = l.ToDictionary(a => a);
            Assert.AreEqual(count, d.Count);
            var orderedKeys = d.Keys.OrderBy().ToList();
            Assert.AreEqual(start, orderedKeys[0]);
        }

        [TestMethod]
        public void ToStringStringKeyValuePairsTest()
        {
            var ins = new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>("a", 1),
                new KeyValuePair<string, object>("b", 2),
                new KeyValuePair<string, object>("b", "jason"),
                new KeyValuePair<string, object>("c", null)
            };
            var outs = ins.ToStringStringKeyValuePairs();
            Assert.AreEqual(ins.Count, outs.Count);
            for (int z = 0; z < ins.Count; ++z)
            {
                var i = ins[z];
                var o = outs[z];
                Assert.AreEqual(i.Key, o.Key);
                if (i.Value == null)
                {
                    Assert.IsNull(o.Value);
                }
                else
                {
                    Assert.AreEqual(i.Value.ToString(), o.Value);
                }
            }
        }

        [TestMethod]
        public void ShuffleTest()
        {
            var a = CreateInOrderList();
            var b = CreateInOrderList();
            ValidateAllElementsExactlyOnce(a);
            ValidateAllElementsExactlyOnce(b);
            b.Shuffle();
            ValidateAllElementsExactlyOnce(b);
            for (int z = 0; z < a.Count; ++z)
            {
                if (a[z] != b[z]) return;
            }
            Assert.Fail();
        }


        [TestMethod]
        public void RandomTest()
        {
            var a = CreateInOrderList(10000);
            for (int z = 0; z < 10; ++z)
            {
                var r1 = a.Random();
                var r2 = a.Random();
                Assert.IsTrue(r1 >= 0 && r1 <= a.Count);
                Assert.IsTrue(r2 >= 0 && r2 <= a.Count);
                Assert.AreNotEqual(r1, r2);
            }
        }
    }
}
