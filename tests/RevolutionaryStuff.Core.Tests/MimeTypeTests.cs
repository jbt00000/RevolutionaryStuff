using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class MimeTypeTests
    {
        private const string MyMimeTypePrimaryType = "PRIME";
        private const string MyMimeTypeSecondaryType = "SECOND";
        private const string MyMimeTypePrimaryExtension = ".hello";
        private const string MyMimeTypeSecondaryExtension = ".world";
        private static readonly MimeType MyMimeType = new MimeType($"{MyMimeTypePrimaryType}/{MyMimeTypeSecondaryType}", MyMimeTypePrimaryExtension, MyMimeTypeSecondaryExtension);

        [TestMethod]
        public void  DoesExtensionMatch_True()
        {
            foreach (var e in new[] {
                "yes"+MyMimeTypePrimaryExtension,
                "yes"+MyMimeTypeSecondaryExtension,


            })
            {
                Assert.IsTrue(MyMimeType.DoesExtensionMatch(e));
                Assert.IsTrue(MyMimeType.DoesExtensionMatch(e.ToLower()));
                Assert.IsTrue(MyMimeType.DoesExtensionMatch(e.ToUpper()));
            }
        }

        [TestMethod]
        public void DoesExtensionMatch_False()
        {
            foreach (var e in new[] {
                "no"+MyMimeTypePrimaryExtension+".duh",
                "no.nah"
            })
            {
                Assert.IsFalse(MyMimeType.DoesExtensionMatch(e));
                Assert.IsFalse(MyMimeType.DoesExtensionMatch(e.ToLower()));
                Assert.IsFalse(MyMimeType.DoesExtensionMatch(e.ToUpper()));
            }
        }

        [TestMethod]
        public void FindWellKnowGlobalTypesByExtension()
        {
            Assert.AreEqual(MimeType.Text.Plain, MimeType.FindByExtension(".text"));
            Assert.AreEqual(MimeType.Text.Plain, MimeType.FindByExtension(".txt"));
            Assert.AreEqual(MimeType.Image.Jpg, MimeType.FindByExtension(".jpg"));
        }
    }
}
