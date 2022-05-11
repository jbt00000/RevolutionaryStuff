using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class CollectionHelpersTests
    {
        [ExpectedException(typeof(Exception), AllowDerivedTypes =true)]
        [TestMethod]
        public void EnumerateNullList()
        {
            IList<int> l = null;
            foreach (var e in l)
            { }
        }

        [TestMethod]
        public void SafeEnumerateNullList()
        {
            IList<int> l = null;
            var cnt = 0;
            foreach (var e in l.NullSafeEnumerable())
            {
                ++cnt;
            }
            Assert.AreEqual(0, cnt);
        }

        [TestMethod]
        public void RemoveWhereNone()
        {
            var items = Enumerable.Range(0, 100).ToList();
            items.Remove(z => false);
            Assert.AreEqual(100, items.Count);
        }

        [TestMethod]
        public void RemoveWhereAll()
        {
            var items = Enumerable.Range(0, 100).ToList();
            items.Remove(z => true);
            Assert.AreEqual(0, items.Count);
        }

        [TestMethod]
        public void RemoveWhereOdd()
        {
            var items = Enumerable.Range(0, 100).ToList();
            items.Remove(z => z.IsOdd());
            Assert.AreEqual(50, items.Count);
        }

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
                new("a", 1),
                new("b", 2),
                new("b", "jason"),
                new("c", null)
            };
            var outs = ins.ToStringStringKeyValuePairs();
            Assert.AreEqual(ins.Count, outs.Count);
            for (var z = 0; z < ins.Count; ++z)
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
            for (var z = 0; z < a.Count; ++z)
            {
                if (a[z] != b[z]) return;
            }
            Assert.Fail();
        }


        [TestMethod]
        public void RandomTest()
        {
            var a = CreateInOrderList(10000);
            for (var z = 0; z < 10; ++z)
            {
                var r1 = a.Random();
                var r2 = a.Random();
                Assert.IsTrue(r1 >= 0 && r1 <= a.Count);
                Assert.IsTrue(r2 >= 0 && r2 <= a.Count);
                Assert.AreNotEqual(r1, r2);
            }
        }

        [TestMethod]
        public void SetIfValNotNull_ValNotNullTest()
        {
            var d = new Dictionary<int, object>();
            d.SetIfValNotNull(0, 1);
            Assert.AreEqual(1, d.Count);
            d.SetIfValNotNull(1, new object());
            Assert.AreEqual(2, d.Count);
            d.SetIfValNotNull(1, "hello");
            Assert.AreEqual(2, d.Count);
        }

        [TestMethod]
        public void SetIfValNotNull_ValIsNullTest()
        {
            var d = new Dictionary<int, object>();
            d.SetIfValNotNull(0, (object)null);
            Assert.AreEqual(0, d.Count);
        }

        [TestMethod]
        public void SetIfKeyNotFound_KeysAreNotAlreadyThere()
        {
            var d = new Dictionary<int, object>();
            d.SetIfKeyNotFound(1, "a");
            d.SetIfKeyNotFound(2, "b");
            d.SetIfKeyNotFound(3, "c");
            Assert.AreEqual(3, d.Count);
        }

        [TestMethod]
        public void SetIfKeyNotFound_KeysAreThere()
        {
            var d = new Dictionary<int, object>();
            d.SetIfKeyNotFound(1, "a");
            d.SetIfKeyNotFound(2, "b");
            d.SetIfKeyNotFound(3, "c");
            Assert.AreEqual(3, d.Count);
            d.SetIfKeyNotFound(1, 2);
            Assert.AreEqual("a", d[1]);
        }
    }
}
