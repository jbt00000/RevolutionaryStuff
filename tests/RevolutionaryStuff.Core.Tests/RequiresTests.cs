using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class RequiresTests
{
    [TestMethod]
    public void SingleCallOnce()
    {
        var called = false;
        Requires.SingleCall(ref called);
    }

    [TestMethod]
    public void SingleCallMultiple()
    {
        var called = false;
        Requires.SingleCall(ref called);
        Assert.Throws<Exception>(() => Requires.SingleCall(ref called));
    }

    [TestMethod]
    public void TextCorrect()
        => Requires.Text("jason", "z");

    [TestMethod]
    public void TextTooShort()
        => Assert.ThrowsExactly<ArgumentException>(() => Requires.Text("tooshort", "z", minLen: 20));

    [TestMethod]
    public void TextTooLong()
        => Assert.ThrowsExactly<ArgumentException>(() => Requires.Text("toolong", "z", maxLen: 3));

    [TestMethod]
    public void TextEmpty()
        => Assert.Throws<Exception>(() => Requires.Text("", "z"));

    [TestMethod]
    public void TextNull()
        => Assert.Throws<Exception>(() => Requires.Text(null, "z"));

    [TestMethod]
    public void NullValid()
        => Requires.Null(null, "hasnodata");

    [TestMethod]
    public void NullInvalid()
        => Assert.Throws<ArgumentException>(() => Requires.Null("I'm supposed to be null", "hasdata"));

    [TestMethod]
    public void NonNullInvalidCheckingCallerArgumentExpression()
    {
        string myVariableName = null;
        try
        {
            ArgumentNullException.ThrowIfNull(myVariableName);
            Assert.Fail($"Expected {nameof(ArgumentNullException)}");
        }
        catch (ArgumentNullException anex)
        {
            Assert.AreEqual(nameof(myVariableName), anex.ParamName);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected {nameof(ArgumentNullException)} instead of {ex.GetType().Name}");
        }
    }

    [TestMethod]
    public void XmlWithNonXmlData()
        => Assert.Throws<Exception>(() => Requires.Xml("honey, I don't think this is xml!", "nonxml"));

    [TestMethod]
    public void XmlWithBoringXmlData()
        => Requires.Xml("<root>yeah, we found xml!</root>", "xml");
}
