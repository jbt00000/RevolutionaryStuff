using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class DateHelpersTests
    {
        [TestMethod]
        public void TestAgeOffsetToday()
        {
            var today = DateTime.Today;
            const int expected = 45;

            var dt45 = today.AddYears(-expected);
            Assert.AreEqual(today.Year - expected, dt45.Year);
            Assert.AreEqual(today.Month, dt45.Month);
            Assert.AreEqual(today.Day, dt45.Day);
            Assert.AreEqual(expected, dt45.Age());
            Assert.AreEqual(expected, dt45.AddDays(-1).Age());
            Assert.AreEqual(expected, dt45.AddMonths(-1).Age());
            Assert.AreEqual(expected - 1, dt45.AddDays(1).Age());
            Assert.AreEqual(expected - 1, dt45.AddMonths(1).Age());
            Assert.AreEqual(expected - 1, dt45.AddMonths(1).AddDays(1+today.Day).Age());
        }
    }
}
