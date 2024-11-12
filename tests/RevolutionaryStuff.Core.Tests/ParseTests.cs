using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class ParseTests
{
    private static void TryParseBoolCheck(string s, bool expectedValue, bool expectedParseSuccess)
    {
        var success = Parse.TryParseBool(s, out var parsedVal);
        Assert.AreEqual(expectedParseSuccess, success);
        if (success)
        {
            Assert.AreEqual(expectedValue, parsedVal);
        }
        else
        {
            Assert.AreEqual(false, parsedVal);
        }
    }


    [TestMethod]
    public void TryParseBoolValTrue()
    {
        TryParseBoolCheck("true", true, true);
        TryParseBoolCheck("1", true, true);
        TryParseBoolCheck("True", true, true);
        TryParseBoolCheck(" true    ", true, true);
        TryParseBoolCheck("     1            ", true, true);
        TryParseBoolCheck("   True  ", true, true);
    }

    [TestMethod]
    public void TryParseBoolValFalse()
    {
        TryParseBoolCheck("false", false, true);
        TryParseBoolCheck("0", false, true);
        TryParseBoolCheck("False", false, true);
        TryParseBoolCheck(" false    ", false, true);
        TryParseBoolCheck("     0            ", false, true);
        TryParseBoolCheck("   False  ", false, true);
    }

    [TestMethod]
    public void TryParseBoolValInvalid()
    {
        TryParseBoolCheck("x", false, false);
        TryParseBoolCheck("fal se", false, false);
        TryParseBoolCheck("tr ue", false, false);
        TryParseBoolCheck("", false, false);
        TryParseBoolCheck(" ", false, false);
        TryParseBoolCheck(null, false, false);
    }
}
