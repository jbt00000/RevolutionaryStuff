using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class StringHelperTests
    {
        [TestMethod]
        public void Base64EncodeDecode()
        {
            foreach (var s in new[] { "1", "22", "333", "444", "55555", "666666", "7777777", "88888888", "999999999"})
            {
                var base64 = s.ToBase64String();
                Assert.IsTrue(base64.Length >= s.Length);
                var decoded = base64.DecodeBase64String();
                Assert.AreEqual(s, decoded);
            }
        }

        [TestMethod]
        public void Base64Strings2Ways()
        {
            foreach (var item in new[] {
                new {
                    s ="Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin ac posuere dui. Maecenas gravida congue quam, ut mollis eros ultrices eget. Vivamus mattis dui eros, at pulvinar erat scelerisque id. Nullam ac interdum purus, ut tincidunt mauris. Aliquam molestie eget magna eget sollicitudin. Duis sollicitudin tellus justo, a imperdiet ipsum pretium non. Sed sodales, dui ut posuere congue, neque odio ullamcorper sem, quis blandit sem nisi id urna. Duis eget mattis magna. Vestibulum posuere, nisi at molestie tincidunt, erat ipsum gravida odio, vitae faucibus orci nunc vitae massa. Donec tristique lorem at felis tristique imperdiet. Nullam mattis, elit eu iaculis suscipit, augue odio consequat erat, at maximus augue tellus ac quam. Mauris id condimentum orci, id semper libero. Pellentesque fringilla nisi ante, sit amet aliquam nulla pretium quis. Etiam sit amet bibendum ligula, nec aliquam risus.",
                    b =@"TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdC4g
UHJvaW4gYWMgcG9zdWVyZSBkdWkuIE1hZWNlbmFzIGdyYXZpZGEgY29uZ3VlIHF1YW0sIHV0IG1v
bGxpcyBlcm9zIHVsdHJpY2VzIGVnZXQuIFZpdmFtdXMgbWF0dGlzIGR1aSBlcm9zLCBhdCBwdWx2
aW5hciBlcmF0IHNjZWxlcmlzcXVlIGlkLiBOdWxsYW0gYWMgaW50ZXJkdW0gcHVydXMsIHV0IHRp
bmNpZHVudCBtYXVyaXMuIEFsaXF1YW0gbW9sZXN0aWUgZWdldCBtYWduYSBlZ2V0IHNvbGxpY2l0
dWRpbi4gRHVpcyBzb2xsaWNpdHVkaW4gdGVsbHVzIGp1c3RvLCBhIGltcGVyZGlldCBpcHN1bSBw
cmV0aXVtIG5vbi4gU2VkIHNvZGFsZXMsIGR1aSB1dCBwb3N1ZXJlIGNvbmd1ZSwgbmVxdWUgb2Rp
byB1bGxhbWNvcnBlciBzZW0sIHF1aXMgYmxhbmRpdCBzZW0gbmlzaSBpZCB1cm5hLiBEdWlzIGVn
ZXQgbWF0dGlzIG1hZ25hLiBWZXN0aWJ1bHVtIHBvc3VlcmUsIG5pc2kgYXQgbW9sZXN0aWUgdGlu
Y2lkdW50LCBlcmF0IGlwc3VtIGdyYXZpZGEgb2Rpbywgdml0YWUgZmF1Y2lidXMgb3JjaSBudW5j
IHZpdGFlIG1hc3NhLiBEb25lYyB0cmlzdGlxdWUgbG9yZW0gYXQgZmVsaXMgdHJpc3RpcXVlIGlt
cGVyZGlldC4gTnVsbGFtIG1hdHRpcywgZWxpdCBldSBpYWN1bGlzIHN1c2NpcGl0LCBhdWd1ZSBv
ZGlvIGNvbnNlcXVhdCBlcmF0LCBhdCBtYXhpbXVzIGF1Z3VlIHRlbGx1cyBhYyBxdWFtLiBNYXVy
aXMgaWQgY29uZGltZW50dW0gb3JjaSwgaWQgc2VtcGVyIGxpYmVyby4gUGVsbGVudGVzcXVlIGZy
aW5naWxsYSBuaXNpIGFudGUsIHNpdCBhbWV0IGFsaXF1YW0gbnVsbGEgcHJldGl1bSBxdWlzLiBF
dGlhbSBzaXQgYW1ldCBiaWJlbmR1bSBsaWd1bGEsIG5lYyBhbGlxdWFtIHJpc3VzLg=="
                }
            })
            {
                Assert.AreEqual(item.s, item.b.DecodeBase64String());
                Assert.AreEqual(item.s, item.s.ToBase64String().DecodeBase64String());
            }
        }

        [TestMethod]
        public void LeftOfTestA()
        {
            Assert.AreEqual("Jason", "Jason Thomas".LeftOf(" "));
        }

        [TestMethod]
        public void LeftOfTestB()
        {
            Assert.AreEqual("Jason", "Jason Thomas".LeftOf(" Th"));
        }

        [TestMethod]
        public void LeftOfTestC()
        {
            Assert.AreEqual("Jason Thomas", "Jason Thomas".LeftOf("zzz"));
        }

        [TestMethod]
        public void RightOfTestA()
        {
            Assert.AreEqual("Thomas", "Jason Thomas".RightOf(" "));
        }

        [TestMethod]
        public void RightOfTestB()
        {
            Assert.AreEqual("omas", "Jason Thomas".RightOf(" Th"));
        }

        [TestMethod]
        public void RightOfTestC()
        {
            Assert.AreEqual(null, "Jason Thomas".RightOf("zzz"));
        }

        [TestMethod]
        public void TrimOrNullTestNoPadding()
        {
            Assert.AreEqual("hello", "hello".TrimOrNull());
        }

        [TestMethod]
        public void TrimOrNullTestPaddingWithInsideSpacing()
        {
            Assert.AreEqual("hello world", " hello world ".TrimOrNull());
        }

        [TestMethod]
        public void TrimOrNullTestAllSpaces()
        {
            Assert.AreEqual(null, "  ".TrimOrNull());
        }

        [TestMethod]
        public void TrimOrNullTestTabs()
        {
            Assert.AreEqual(null, "   ".TrimOrNull());
        }

        [TestMethod]
        public void TruncateWithEllipsisTestTruncation()
        {
            Assert.AreEqual("hello...", "hello world".TruncateWithEllipsis(8));
        }

        [TestMethod]
        public void TruncateWithEllipsisTestNoTruncation()
        {
            Assert.AreEqual("hello world", "hello world".TruncateWithEllipsis(20));
        }

        [TestMethod]
        public void TruncateWithEllipsisTestExactLength()
        {
            var test = "hello world";
            Assert.AreEqual(test, test.TruncateWithEllipsis(test.Length));
        }

        [TestMethod]
        public void TruncateWithEllipsisTestLongerTruncation()
        {
            var test = "hello world";
            Assert.AreEqual(test, test.TruncateWithEllipsis(test.Length + 1));
        }
        [TestMethod]
        public void TruncateWithEllipsisTestShorterTruncation()
        {
            var test = "hello world";
            var res = test.TruncateWithEllipsis(test.Length - 1);
            Assert.AreNotEqual(test, res);
            Assert.AreEqual(test.Length - 1, res.Length);
        }

        [TestMethod]
        public void TruncateWithEllipsisTestDifferentEllipsis()
        {
            Assert.AreEqual("hello ,,", "hello world".TruncateWithEllipsis(8, ",,"));
        }

        [TestMethod]
        public void IsSameIgnoreCaseTestSame()
        {
            Assert.IsTrue(StringHelpers.IsSameIgnoreCase("hello", "HELLO"));
        }

        [TestMethod]
        public void IsSameIgnoreCaseTestDifferent()
        {
            Assert.IsFalse(StringHelpers.IsSameIgnoreCase("hello", "HE LO"));
        }

        [TestMethod]
        public void LeftTests()
        {
            Assert.IsNull(StringHelpers.Left(null, 3));
            Assert.AreEqual("", "".Left(3));
            Assert.AreEqual("jas", "jason".Left(3));
            Assert.AreEqual("jason", "jason".Left(300));
        }

        [TestMethod]
        public void RightTests()
        {
            Assert.IsNull(StringHelpers.Right(null, 3));
            Assert.AreEqual("", "".Right(3));
            Assert.AreEqual("son", "jason".Right(3));
            Assert.AreEqual("jason", "jason".Right(300));
        }
    }
}
