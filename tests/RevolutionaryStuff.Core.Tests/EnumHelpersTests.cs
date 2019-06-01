using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class EnumHelpersTests
    {
        [TestMethod]
        public void EnumRandomTest()
        {
            for (int z = 0; z < 100; ++z)
            {
                var a = EnumHelpers.Random<EItemTypes>();
                var b = EnumHelpers.Random<EItemTypes>();
                var c = EnumHelpers.Random<EItemTypes>();
                var d = EnumHelpers.Random<EItemTypes>();
                Assert.IsFalse(a == b && a == c && a == d);
            }
        }

        private enum EItemTypes
        {
            a = 1,
            b = 2,
            c = 3,
            d = 4,
            e = 5,
            f = 6,
            g = 7,
            h = 8,
            i = 9,
            j = 10,
        }
    }
}
