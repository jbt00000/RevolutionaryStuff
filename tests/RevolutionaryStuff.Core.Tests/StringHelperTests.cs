using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class StringHelperTests
{
    #region Base64 Encoding Tests

    [TestMethod]
    public void Base64EncodeDecode()
    {
        foreach (var s in new[] { "1", "22", "333", "444", "55555", "666666", "7777777", "88888888", "999999999" })
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
    public void ToBase64String_WithNull_ReturnsNull()
    {
        string s = null;
        Assert.IsNull(s.ToBase64String());
    }

    [TestMethod]
    public void DecodeBase64String_WithNull_ReturnsNull()
    {
        string s = null;
        Assert.IsNull(s.DecodeBase64String());
    }

    #endregion

    #region LeftOf/RightOf Tests

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
    public void LeftOf_WithNull_ReturnsNull()
    {
        string s = null;
        Assert.IsNull(s.LeftOf(" "));
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
    public void RightOf_WithReturnFullString_ReturnsFullString()
    {
        Assert.AreEqual("Jason Thomas", "Jason Thomas".RightOf("zzz", returnFullStringIfPivotIsMissing: true));
    }

    [TestMethod]
    public void RightOf_WithNull_ReturnsNull()
    {
        string s = null;
        Assert.IsNull(s.RightOf(" "));
    }

    #endregion

    #region TrimOrNull Tests

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
    public void TrimOrNull_WithMaxLength_TruncatesCorrectly()
    {
        Assert.AreEqual("hel", "  hello  ".TrimOrNull(maxLength: 3));
    }

    [TestMethod]
    public void TrimOrNull_WithNull_ReturnsNull()
    {
        string s = null;
        Assert.IsNull(s.TrimOrNull());
    }

    #endregion

    #region TruncateWithEllipsis Tests

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
    public void TruncateWithEllipsis_WithNull_ReturnsNull()
    {
        string s = null;
        Assert.IsNull(s.TruncateWithEllipsis(10));
    }

    [TestMethod]
    public void TruncateWithEllipsis_WithZeroLength_ReturnsEmpty()
    {
        Assert.AreEqual("", "hello".TruncateWithEllipsis(0));
    }

    #endregion

    #region TruncateWithMidlineEllipsis Tests

    [TestMethod]
    public void TruncateWithMidlineEllipsis_Basic()
    {
        var result = "HelloWorld".TruncateWithMidlineEllipsis(8, "...");
        Assert.AreEqual(8, result.Length);
        Assert.IsTrue(result.Contains("..."));
    }

    [TestMethod]
    public void TruncateWithMidlineEllipsis_NoTruncation()
    {
        Assert.AreEqual("Hello", "Hello".TruncateWithMidlineEllipsis(10));
    }

    [TestMethod]
    public void TruncateWithMidlineEllipsis_WithNull_ReturnsNull()
    {
        string s = null;
        Assert.IsNull(s.TruncateWithMidlineEllipsis(10));
    }

    #endregion

    #region IsSameIgnoreCase Tests

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
    public void IsSameIgnoreCase_WithNull_HandlesCorrectly()
    {
        Assert.IsTrue(StringHelpers.IsSameIgnoreCase(null, null));
        Assert.IsFalse(StringHelpers.IsSameIgnoreCase("hello", null));
        Assert.IsFalse(StringHelpers.IsSameIgnoreCase(null, "hello"));
    }

    #endregion

    #region Left/Right Tests

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

    #endregion

    #region CondenseWhitespace Tests

    [TestMethod]
    public void CondenseWhitespace_MultipleSpaces()
    {
        Assert.AreEqual("hello world", StringHelpers.CondenseWhitespace("hello  world"));
    }

    [TestMethod]
    public void CondenseWhitespace_MixedWhitespace()
    {
        Assert.AreEqual("hello world", StringHelpers.CondenseWhitespace("hello \t\n world"));
    }

    [TestMethod]
    public void CondenseWhitespace_WithNull_ReturnsNull()
    {
        Assert.IsNull(StringHelpers.CondenseWhitespace(null));
    }

    #endregion

    #region Case Conversion Tests

    [TestMethod]
    public void ToTitleFriendlyString_Basic()
    {
        Assert.AreEqual("My Property Name", "MyPropertyName".ToTitleFriendlyString());
    }

    [TestMethod]
    public void ToTitleFriendlyString_WithUnderscores()
    {
        Assert.AreEqual("My Property Name", "my_property_name".ToTitleFriendlyString());
    }

    [TestMethod]
    public void ToUpperCamelCase_Basic()
    {
        Assert.AreEqual("HelloWorld", "hello world".ToUpperCamelCase());
    }

    [TestMethod]
    public void ToLowerCamelCase_Basic()
    {
        Assert.AreEqual("helloWorld", "hello world".ToLowerCamelCase());
    }

    [TestMethod]
    public void ToTitleCase_Basic()
    {
        Assert.AreEqual("Hello World", "hello world".ToTitleCase());
    }

    [TestMethod]
    public void ToTitleCase_WithEmpty_ReturnsEmpty()
    {
        Assert.AreEqual("", "".ToTitleCase());
    }

    #endregion

    #region ASCII Tests

    [TestMethod]
    public void ContainsOnlyAsciiCharacters_AllAscii()
    {
        Assert.IsTrue("Hello123".ContainsOnlyAsciiCharacters());
    }

    [TestMethod]
    public void ContainsOnlyAsciiCharacters_WithUnicode()
    {
        Assert.IsFalse("Hello世界".ContainsOnlyAsciiCharacters());
    }

    [TestMethod]
    public void ContainsOnlyExtendedAsciiCharacters_Extended()
    {
        Assert.IsTrue("Héllo".ContainsOnlyExtendedAsciiCharacters());
    }

    [TestMethod]
    public void ContainsOnlyExtendedAsciiCharacters_WithUnicode()
    {
        Assert.IsFalse("Hello世界".ContainsOnlyExtendedAsciiCharacters());
    }

    #endregion

    #region AppendFormat Tests

    [TestMethod]
    public void AppendFormatIfValNotNull_WithValue()
    {
        var result = "Hello".AppendFormatIfValNotNull(" {0}", "World");
        Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void AppendFormatIfValNotNull_WithNullValue()
    {
        var result = "Hello".AppendFormatIfValNotNull(" {0}", null);
        Assert.AreEqual("Hello", result);
    }

    [TestMethod]
    public void AppendFormatIfValNotNull_WithNullBase()
    {
        string s = null;
        var result = s.AppendFormatIfValNotNull("{0}", "World");
        Assert.AreEqual("World", result);
    }

    #endregion

    #region AppendWithConditionalAppendPrefix Tests

    [TestMethod]
    public void AppendWithConditionalAppendPrefix_BothNonEmpty()
    {
        var result = "Hello".AppendWithConditionalAppendPrefix(", ", "World");
        Assert.AreEqual("Hello, World", result);
    }

    [TestMethod]
    public void AppendWithConditionalAppendPrefix_EmptyBase()
    {
        var result = "".AppendWithConditionalAppendPrefix(", ", "World");
        Assert.AreEqual("World", result);
    }

    [TestMethod]
    public void AppendWithConditionalAppendPrefix_NullBase()
    {
        string s = null;
        var result = s.AppendWithConditionalAppendPrefix(", ", "World");
        Assert.AreEqual("World", result);
    }

    #endregion

    #region Split Tests

    [TestMethod]
    public void Split_Found()
    {
        var found = "1234567".Split("34", true, out var left, out var right);
        Assert.IsTrue(found);
        Assert.AreEqual("12", left);
        Assert.AreEqual("567", right);
    }

    [TestMethod]
    public void Split_NotFound()
    {
        var found = "1234567".Split("99", true, out var left, out var right);
        Assert.IsFalse(found);
        Assert.AreEqual("1234567", left);
        Assert.AreEqual("", right);
    }

    [TestMethod]
    public void Split_LastOccurrence()
    {
        var found = "123412345".Split("34", false, out var left, out var right);
        Assert.IsTrue(found);
        Assert.AreEqual("123412", left);  // Last occurrence of "34" is at position 5
        Assert.AreEqual("5", right);
    }

    #endregion

    #region Contains Tests

    [TestMethod]
    public void Contains_CaseSensitive_Found()
    {
        Assert.IsTrue("Hello World".Contains("World"));
    }

    [TestMethod]
    public void Contains_CaseSensitive_NotFound()
    {
        Assert.IsFalse("Hello World".Contains("world"));
    }

    [TestMethod]
    public void Contains_CaseInsensitive_Found()
    {
        Assert.IsTrue("Hello World".Contains("world", ignoreCase: true));
    }

    [TestMethod]
    public void Contains_WithNull_ReturnsFalse()
    {
        string s = null;
        Assert.IsFalse(s.Contains("test", ignoreCase: false));
    }

    #endregion

    #region RemoveSpecialCharacters Tests

    [TestMethod]
    public void RemoveSpecialCharacters_MixedContent()
    {
        Assert.AreEqual("Hello123World", "Hello@#123$%World!".RemoveSpecialCharacters());
    }

    [TestMethod]
    public void RemoveSpecialCharacters_OnlyAlphanumeric()
    {
        Assert.AreEqual("Hello123", "Hello123".RemoveSpecialCharacters());
    }

    #endregion

    #region RemoveDiacritics Tests

    [TestMethod]
    public void RemoveDiacritics_FrenchAccents()
    {
        Assert.AreEqual("cafe", "café".RemoveDiacritics());
    }

    [TestMethod]
    public void RemoveDiacritics_GermanUmlaut()
    {
        Assert.AreEqual("Muller", "Müller".RemoveDiacritics());
    }

    [TestMethod]
    public void RemoveDiacritics_SpanishTilde()
    {
        Assert.AreEqual("nino", "niño".RemoveDiacritics());
    }

    #endregion

    #region Coalesce Tests

    [TestMethod]
    public void Coalesce_ReturnsFirstNonEmpty()
    {
        Assert.AreEqual("second", StringHelpers.Coalesce(null, "", "  ", "second", "third"));
    }

    [TestMethod]
    public void Coalesce_AllEmpty_ReturnsNull()
    {
        Assert.IsNull(StringHelpers.Coalesce(null, "", "  "));
    }

    [TestMethod]
    public void Coalesce_WithNull_ReturnsNull()
    {
        Assert.IsNull(StringHelpers.Coalesce(null));
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_WithValue_ReturnsString()
    {
        Assert.AreEqual("42", 42.ToString((string)null));
    }

    [TestMethod]
    public void ToString_WithNull_ReturnsNullValue()
    {
        object o = null;
        Assert.AreEqual("default", o.ToString("default"));
    }

    [TestMethod]
    public void ToString_WithNullNoDefault_ReturnsNull()
    {
        object o = null;
        Assert.IsNull(o.ToString(nullValue: null));
    }

    #endregion

    #region NullSafeHasData Tests

    [TestMethod]
    public void NullSafeHasData_WithData()
    {
        Assert.IsTrue("hello".NullSafeHasData());
    }

    [TestMethod]
    public void NullSafeHasData_Empty()
    {
        Assert.IsFalse("".NullSafeHasData());
    }

    [TestMethod]
    public void NullSafeHasData_Null()
    {
        string s = null;
        Assert.IsFalse(s.NullSafeHasData());
    }

    #endregion

    #region FormatWithNamedArgs Tests

    [TestMethod]
    public void FormatWithNamedArgs_SingleArg()
    {
        var result = StringHelpers.FormatWithNamedArgs("Hello {name}!", "name", "World");
        Assert.AreEqual("Hello World!", result);
    }

    [TestMethod]
    public void FormatWithNamedArgs_MultipleArgs()
    {
        var args = new[]
        {
            new System.Collections.Generic.KeyValuePair<string, object>("name", "Alice"),
            new System.Collections.Generic.KeyValuePair<string, object>("age", 30)
        };
        var result = StringHelpers.FormatWithNamedArgs("Name: {name}, Age: {age}", args);
        Assert.AreEqual("Name: Alice, Age: 30", result);
    }

    [TestMethod]
    public void FormatWithNamedArgs_WithModifiers()
    {
        var args = new[]
        {
            new System.Collections.Generic.KeyValuePair<string, object>("value", 3.14159)
        };
        var result = StringHelpers.FormatWithNamedArgs("Pi: {value:F2}", args);
        Assert.AreEqual("Pi: 3.14", result);
    }

    [TestMethod]
    public void FormatWithNamedArgs_MissingValue()
    {
        var result = StringHelpers.FormatWithNamedArgs("Hello {name}!", Array.Empty<System.Collections.Generic.KeyValuePair<string, object>>(), missingVal: "Unknown");
        Assert.AreEqual("Hello Unknown!", result);
    }

    #endregion

    #region Regex Split Tests

    [TestMethod]
    public void Split_WithRegex()
    {
        var regex = new Regex(@"\s+");
        var result = "hello  world   test".Split(regex);
        Assert.HasCount(3, result);
        Assert.AreEqual("hello", result[0]);
        Assert.AreEqual("world", result[1]);
        Assert.AreEqual("test", result[2]);
    }

    [TestMethod]
    public void Split_WithNullRegex_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => "test".Split((Regex)null));
    }

    #endregion

    #region Replace with Match Tests

    [TestMethod]
    public void Replace_WithSuccessfulMatch()
    {
        var regex = new Regex(@"\d+");
        var match = regex.Match("abc123def");
        var result = "abc123def".Replace(match, "XXX");
        Assert.AreEqual("abcXXXdef", result);
    }

    [TestMethod]
    public void Replace_WithFailedMatch()
    {
        var regex = new Regex(@"\d+");
        var match = regex.Match("abcdef");
        var result = "abcdef".Replace(match, "XXX");
        Assert.AreEqual("abcdef", result);
    }

    #endregion
}
