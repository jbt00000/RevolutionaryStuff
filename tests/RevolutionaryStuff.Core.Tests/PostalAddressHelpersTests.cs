using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.ApplicationParts.PostalAddress;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class PostalAddressHelpersTests
{
    [TestMethod]
    public void CreateFromFreeform_WithSingleLineAddress_ParsesAddress()
    {
        var address = PostalAddressHelpers.CreateFromFreeform("600 Dexter Avenue, Montgomery, AL 36130");

        Assert.AreEqual("600 Dexter Avenue", address.AddressLine1);
        Assert.IsNull(address.AddressLine2);
        Assert.AreEqual("Montgomery", address.City);
        Assert.AreEqual("AL", address.State);
        Assert.AreEqual("36130", address.PostalCode);
    }

    [TestMethod]
    public void CreateFromFreeform_WithDefaultCountry_SetsCountry()
    {
        var address = PostalAddressHelpers.CreateFromFreeform("600 Dexter Avenue, Montgomery, AL 36130", "US");

        Assert.AreEqual("US", address.Country);
    }

    [TestMethod]
    public void CreateFromFreeform_WithAddressLine2_ParsesMultilineAddress()
    {
        var address = PostalAddressHelpers.CreateFromFreeform("600 Dexter Avenue\nSuite 100\nMontgomery, AL 36130", "US");

        Assert.AreEqual("600 Dexter Avenue", address.AddressLine1);
        Assert.AreEqual("Suite 100", address.AddressLine2);
        Assert.AreEqual("Montgomery", address.City);
        Assert.AreEqual("AL", address.State);
        Assert.AreEqual("36130", address.PostalCode);
        Assert.AreEqual("US", address.Country);
    }
}
