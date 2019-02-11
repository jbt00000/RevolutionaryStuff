using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class CompareHelpersTests
    {
        [TestMethod]
        public void CompareSameNull()
        {
            Assert.IsTrue(CompareHelpers.Compare(null, null));
        }

        [TestMethod]
        public void CompareZeroLen()
        {
            Assert.IsTrue(CompareHelpers.Compare(new byte[0], new byte[0]));
        }

        [TestMethod]
        public void ComparePrefixSameLeftShort()
        {
            Assert.IsFalse(CompareHelpers.Compare(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3, 4, 5 }));
        }

        [TestMethod]
        public void ComparePrefixSameRightShort()
        {
            Assert.IsFalse(CompareHelpers.Compare(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 1, 2, 3 }));
        }

        [TestMethod]
        public void CompareSameLengthDifferentVals()
        {
            Assert.IsFalse(CompareHelpers.Compare(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }));
        }
    }
}
