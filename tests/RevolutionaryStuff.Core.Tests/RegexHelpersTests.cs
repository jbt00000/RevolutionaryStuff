using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class RegexHelpersTests
{
    [DataRow("   ")]
    [DataRow(" \t  ")]
    [DataRow("")]
    [TestMethod]
    public void EmailAddressesEmpty(string email)
    {
        Assert.IsFalse(RegexHelpers.Common.EmailAddress.IsMatch(email));
        Assert.IsFalse(RegexHelpers.Common.EmailAddress.IsMatch(email?.ToUpper()));
        Assert.IsFalse(RegexHelpers.Common.EmailAddress.IsMatch(email?.ToLower()));
    }

    [DataRow("BlEH")]
    [TestMethod]
    public void EmailAddressesInvalid(string email)
    {
        Assert.IsFalse(RegexHelpers.Common.EmailAddress.IsMatch(email));
        Assert.IsFalse(RegexHelpers.Common.EmailAddress.IsMatch(email?.ToUpper()));
        Assert.IsFalse(RegexHelpers.Common.EmailAddress.IsMatch(email?.ToLower()));
    }


    [DataRow("jason@jasonthomas.com")]
    [DataRow("jason@traffk.com")]
    [TestMethod]
    public void EmailAddressesValid(string email)
    {
        Assert.IsTrue(RegexHelpers.Common.EmailAddress.IsMatch(email));
        Assert.IsTrue(RegexHelpers.Common.EmailAddress.IsMatch(email?.ToUpper()));
        Assert.IsTrue(RegexHelpers.Common.EmailAddress.IsMatch(email?.ToLower()));
    }

    [DataRow("jason+test@jasonthomas.com")]
    [DataRow("jason+test@traffk.com")]
    [TestMethod]
    public void EmailAddressesWithPlusAddressingValid(string email)
    {
        Assert.IsTrue(RegexHelpers.Common.EmailAddress.IsMatch(email));
        Assert.IsTrue(RegexHelpers.Common.EmailAddress.IsMatch(email?.ToUpper()));
        Assert.IsTrue(RegexHelpers.Common.EmailAddress.IsMatch(email?.ToLower()));
    }
}
