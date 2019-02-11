using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class OtherExtensionsTests
    {
        #region Even Odd Tests
        [TestMethod]
        public void IsOddZeroTest()
            => Assert.IsFalse(0.IsOdd());

        [TestMethod]
        public void IsOddPositiveOddTest()
            => Assert.IsTrue(5.IsOdd());

        [TestMethod]
        public void IsOddNegativeOddTest()
            => Assert.IsTrue((-5).IsOdd());

        [TestMethod]
        public void IsOddPositiveEvenTest()
            => Assert.IsFalse(6.IsOdd());

        [TestMethod]
        public void IsOddNegativeEvenTest()
            => Assert.IsFalse((-6).IsOdd());

        [TestMethod]
        public void IsEvenZeroTest()
            => Assert.IsTrue(0.IsEven());

        [TestMethod]
        public void IsEvenPositiveOddTest()
            => Assert.IsFalse(5.IsEven());

        [TestMethod]
        public void IsEvenNegativeOddTest()
            => Assert.IsFalse((-5).IsEven());

        [TestMethod]
        public void IsEvenPositiveEvenTest()
            => Assert.IsTrue(6.IsEven());

        [TestMethod]
        public void IsEvenNegativeEvenTest()
            => Assert.IsTrue((-6).IsEven());
        #endregion

        [TestMethod]
        public void IsWeekendTest()
        {
            Assert.IsFalse(new DateTime(2019, 1, 1).IsWeekend()); //tuesday
            Assert.IsFalse(new DateTime(2019, 1, 2).IsWeekend());
            Assert.IsFalse(new DateTime(2019, 1, 3).IsWeekend());
            Assert.IsFalse(new DateTime(2019, 1, 4).IsWeekend());
            Assert.IsTrue(new DateTime(2019, 1, 5).IsWeekend()); //saturday
            Assert.IsTrue(new DateTime(2019, 1, 6).IsWeekend()); //sunday
            Assert.IsFalse(new DateTime(2019, 1, 7).IsWeekend());
        }

        [TestMethod]
        public void IsWeekdayTest()
        {
            Assert.IsTrue(new DateTime(2019, 1, 1).IsWeekday()); //tuesday
            Assert.IsTrue(new DateTime(2019, 1, 2).IsWeekday());
            Assert.IsTrue(new DateTime(2019, 1, 3).IsWeekday());
            Assert.IsTrue(new DateTime(2019, 1, 4).IsWeekday());
            Assert.IsFalse(new DateTime(2019, 1, 5).IsWeekday()); //saturday
            Assert.IsFalse(new DateTime(2019, 1, 6).IsWeekday()); //sunday
            Assert.IsTrue(new DateTime(2019, 1, 7).IsWeekday());
        }
    }
}
