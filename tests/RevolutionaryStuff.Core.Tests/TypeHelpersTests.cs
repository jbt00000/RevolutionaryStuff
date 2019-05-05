using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class TypeHelpersTests
    {
        [TestMethod]
        public void ConvertFromUriString()
            => Assert.AreEqual(new Uri("http://www.espn.com"), TypeHelpers.ConvertValue(typeof(Uri), "http://www.espn.com"));

        [TestMethod]
        public void ConvertFromBoolTrueString()
            => Assert.AreEqual(true, TypeHelpers.ConvertValue(typeof(bool), "true"));

        [TestMethod]
        public void ConvertFromBool1String()
            => Assert.AreEqual(true, TypeHelpers.ConvertValue(typeof(bool), "1"));

        [TestMethod]
        public void ConvertFromBoolFalseString()
            => Assert.AreEqual(false, TypeHelpers.ConvertValue(typeof(bool), "false"));

        [TestMethod]
        public void ConvertFromBool0String()
            => Assert.AreEqual(false, TypeHelpers.ConvertValue(typeof(bool), "0"));

        public enum ConvertTestEnum
        {
            a=1,
            b=2,
            c=3
        }

        [TestMethod]
        public void ConvertFromEnumString()
            => Assert.AreEqual(ConvertTestEnum.b, TypeHelpers.ConvertValue(typeof(ConvertTestEnum), "b"));

        [TestMethod]
        public void ConvertFromEnumWrongCaseString()
            => Assert.AreEqual(ConvertTestEnum.b, TypeHelpers.ConvertValue(typeof(ConvertTestEnum), "B"));

        [TestMethod]
        public void ConvertFromEnumNumberString()
            => Assert.AreEqual(ConvertTestEnum.b, TypeHelpers.ConvertValue(typeof(ConvertTestEnum), "2"));

        [TestMethod]
        public void ConvertFromNumber5String()
            => Assert.AreEqual(5, TypeHelpers.ConvertValue(typeof(int), "5"));

        [TestMethod]
        public void ConvertFromNumberNeg7String()
            => Assert.AreEqual(-7, TypeHelpers.ConvertValue(typeof(int), "-7"));
    }
}
