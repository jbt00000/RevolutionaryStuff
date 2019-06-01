using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class RequiresTests
    {
        [TestMethod]
        public void SingleCallOnce()
        {
            bool called = false;
            Requires.SingleCall(ref called);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void SingleCallMultiple()
        {
            bool called = false;
            Requires.SingleCall(ref called);
            Requires.SingleCall(ref called);
        }

        [TestMethod]
        public void TextCorrect()
            => Requires.Text("jason", "z");

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TextTooShort()
            => Requires.Text("tooshort", "z", minLen:20);

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TextTooLong()
            => Requires.Text("toolong", "z", maxLen:3);

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TextEmpty()
            => Requires.Text("", "z");

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TextNull()
            => Requires.Text(null, "z");

        [TestMethod]
        public void NullValid()
            => Requires.Null(null, "hasnodata");

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void NullInvalid()
            => Requires.Null("I'm supposed to be null", "hasdata");

        [TestMethod]
        public void NonNullValid()
            => Requires.NonNull("jason thomas", "hasdata");

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void NonNullInvalid()
            => Requires.NonNull(null, "hasnodata");

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void XmlWithNonXmlData()
            => Requires.Xml("honey, I don't think this is xml!", "nonxml");

        [TestMethod]
        public void XmlWithBoringXmlData()
            => Requires.Xml("<root>yeah, we found xml!</root>", "xml");

        [TestMethod]
        public void XmlWithXsdValid()
            => Requires.Xml(CustomersOrdersXml, "xml", CustomerOrdersXsd, null, false);

        [TestMethod]
        public void XmlWithXsdValidAndCorrectPrimaryNamespaceGiven()
            => Requires.Xml(CustomersOrdersXml, "xml", CustomerOrdersXsd, CustomerOrdersNamespaceUri, false);

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void XmlWithXsdValidAndIncorrectPrimaryNamespaceGiven()
            => Requires.Xml(CustomersOrdersXml, "xml", CustomerOrdersXsd, CustomerOrdersNamespaceUri+"zzz", false);

        private static readonly string CustomerOrdersXsd = ResourceHelpers.GetEmbeddedResourceAsString(Assembly.GetExecutingAssembly(), "CustomerOrders.xsd");

        private const string CustomerOrdersNamespaceUri = "http://www.adventure-works.com";

        private static readonly string CustomersOrdersXml = ResourceHelpers.GetEmbeddedResourceAsString(Assembly.GetExecutingAssembly(), "CustomersOrdersInNamespace.xml");

    }
}
