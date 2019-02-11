using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class StringHelperTests
    {
        [TestMethod]
        public void LeftOfTestA()
        {
            Assert.AreEqual("Jason", "Jason Thomas".LeftOf(" "));
        }

        [TestMethod]
        public void LeftOfTestB()
        {
            Assert.AreEqual("Jason", "Jason Thomas".LeftOf(" Th"));
        }

        [TestMethod]
        public void LeftOfTestC()
        {
            Assert.AreEqual("Jason Thomas", "Jason Thomas".LeftOf("zzz"));
        }

        [TestMethod]
        public void RightOfTestA()
        {
            Assert.AreEqual("Thomas", "Jason Thomas".RightOf(" "));
        }

        [TestMethod]
        public void RightOfTestB()
        {
            Assert.AreEqual("omas", "Jason Thomas".RightOf(" Th"));
        }

        [TestMethod]
        public void RightOfTestC()
        {
            Assert.AreEqual(null, "Jason Thomas".RightOf("zzz"));
        }

        [TestMethod]
        public void TrimOrNullTestNoPadding()
        {
            Assert.AreEqual("hello", "hello".TrimOrNull());
        }

        [TestMethod]
        public void TrimOrNullTestPaddingWithInsideSpacing()
        {
            Assert.AreEqual("hello world", " hello world ".TrimOrNull());
        }

        [TestMethod]
        public void TrimOrNullTestAllSpaces()
        {
            Assert.AreEqual(null, "  ".TrimOrNull());
        }

        [TestMethod]
        public void TrimOrNullTestTabs()
        {
            Assert.AreEqual(null, "   ".TrimOrNull());
        }

        [TestMethod]
        public void TruncateWithEllipsisTestTruncation()
        {
            Assert.AreEqual("hello...", "hello world".TruncateWithEllipsis(8));
        }

        [TestMethod]
        public void TruncateWithEllipsisTestNoTruncation()
        {
            Assert.AreEqual("hello world", "hello world".TruncateWithEllipsis(20));
        }

        [TestMethod]
        public void TruncateWithEllipsisTestDifferentEllipsis()
        {
            Assert.AreEqual("hello ,,", "hello world".TruncateWithEllipsis(8, ",,"));
        }

        [TestMethod]
        public void IsSameIgnoreCaseTestSame()
        {
            Assert.IsTrue(StringHelpers.IsSameIgnoreCase("hello", "HELLO"));
        }

        [TestMethod]
        public void IsSameIgnoreCaseTestDifferent()
        {
            Assert.IsFalse(StringHelpers.IsSameIgnoreCase("hello", "HE LO"));
        }

        [TestMethod]
        public void LeftTests()
        {
            Assert.IsNull(StringHelpers.Left(null, 3));
            Assert.AreEqual("", "".Left(3));
            Assert.AreEqual("jas", "jason".Left(3));
            Assert.AreEqual("jason", "jason".Left(300));
        }

        [TestMethod]
        public void RightTests()
        {
            Assert.IsNull(StringHelpers.Right(null, 3));
            Assert.AreEqual("", "".Right(3));
            Assert.AreEqual("son", "jason".Right(3));
            Assert.AreEqual("jason", "jason".Right(300));
        }
    }
}
