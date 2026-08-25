using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class JsonHelpersUnJsonElementTests
{
    private static JsonElement Parse(string json)
        => JsonDocument.Parse(json).RootElement;

    [TestMethod]
    public void ToPoco_Null_ReturnsNull()
    {
        var jel = Parse("null");
        var result = JsonHelpers.ToPoco(jel);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ToPoco_True_ReturnsBoolTrue()
    {
        var jel = Parse("true");
        var result = JsonHelpers.ToPoco(jel);
        Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void ToPoco_False_ReturnsBoolFalse()
    {
        var jel = Parse("false");
        var result = JsonHelpers.ToPoco(jel);
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void ToPoco_PlainString_ReturnsString()
    {
        var jel = Parse("\"hello world\"");
        var result = JsonHelpers.ToPoco(jel);
        Assert.AreEqual("hello world", result);
    }

    [TestMethod]
    public void ToPoco_GuidLikeString_DefaultBehavior_ReturnsGuid()
    {
        var g = Guid.NewGuid();
        var jel = Parse($"\"{g}\"");
        var result = JsonHelpers.ToPoco(jel);
        Assert.IsInstanceOfType(result, typeof(Guid));
        Assert.AreEqual(g, result);
    }

    [TestMethod]
    public void ToPoco_IntegerNumber_PopularTypes_ReturnsInt()
    {
        var jel = Parse("42");
        var result = JsonHelpers.ToPoco(jel);
        Assert.IsInstanceOfType(result, typeof(int));
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ToPoco_LargeIntegerNumber_ReturnsLong()
    {
        var jel = Parse(long.MaxValue.ToString());
        var result = JsonHelpers.ToPoco(jel);
        Assert.IsInstanceOfType(result, typeof(long));
        Assert.AreEqual(long.MaxValue, result);
    }

    [TestMethod]
    public void ToPoco_FractionalNumber_ReturnsDouble()
    {
        var jel = Parse("3.14");
        var result = JsonHelpers.ToPoco(jel);
        Assert.IsInstanceOfType(result, typeof(double));
        Assert.AreEqual(3.14, result);
    }

    [TestMethod]
    public void ToPoco_FitToSize_SmallInteger_ReturnsInt16()
    {
        var jel = Parse("42");
        var settings = new JsonHelpers.ToPocoSettings
        {
            NumericConversionStrategy = JsonHelpers.ToPocoSettings.NumericConversionStrategyEnum.FitToSize
        };
        var result = JsonHelpers.ToPoco(jel, settings);
        Assert.IsInstanceOfType(result, typeof(short));
    }

    [TestMethod]
    public void ToPoco_Array_ReturnsList()
    {
        var jel = Parse("[1, 2, 3]");
        var result = JsonHelpers.ToPoco(jel);
        var list = result as List<object>;
        Assert.IsNotNull(list);
        CollectionAssert.AreEqual(new object[] { 1, 2, 3 }, list);
    }

    [TestMethod]
    public void ToPoco_Object_ReturnsDictionary()
    {
        var jel = Parse("""{"Name":"Bob","Age":30}""");
        var result = JsonHelpers.ToPoco(jel);
        var dict = result as Dictionary<string, object>;
        Assert.IsNotNull(dict);
        Assert.AreEqual("Bob", dict["Name"]);
        Assert.AreEqual(30, dict["Age"]);
    }

    [TestMethod]
    public void ToPoco_NestedObjectAndArray_ConvertsRecursively()
    {
        var jel = Parse("""{"items":[{"a":1},{"a":2}]}""");
        var result = JsonHelpers.ToPoco(jel) as Dictionary<string, object>;
        Assert.IsNotNull(result);
        var items = result["items"] as List<object>;
        Assert.IsNotNull(items);
        Assert.AreEqual(2, items.Count);
        var first = items[0] as Dictionary<string, object>;
        Assert.IsNotNull(first);
        Assert.AreEqual(1, first["a"]);
    }

    [TestMethod]
    public void ToPoco_ObjectDictionary_DefaultComparerIsCaseSensitive()
    {
        var jel = Parse("""{"Name":"Bob"}""");
        var result = JsonHelpers.ToPoco(jel) as Dictionary<string, object>;
        Assert.IsNotNull(result);
        Assert.IsFalse(result.ContainsKey("name"));
    }

    [TestMethod]
    public void ToPoco_ObjectDictionary_CustomComparerIsHonored()
    {
        var jel = Parse("""{"Name":"Bob"}""");
        var settings = new JsonHelpers.ToPocoSettings
        {
            DictionaryComparer = StringComparer.OrdinalIgnoreCase
        };
        var result = JsonHelpers.ToPoco(jel, settings) as Dictionary<string, object>;
        Assert.IsNotNull(result);
        Assert.IsTrue(result.ContainsKey("name"));
        Assert.AreEqual("Bob", result["name"]);
    }

    [TestMethod]
    public void ToPoco_NonJsonElementObject_ReturnsUnchanged()
    {
        object o = "just a string";
        var result = JsonHelpers.ToPoco(o);
        Assert.AreSame(o, result);
    }

    [TestMethod]
    public void ToPoco_JsonElementOverload_MatchesObjectOverload()
    {
        var jel = Parse("""{"Name":"Bob"}""");
        var fromObjectOverload = JsonHelpers.ToPoco((object) jel) as Dictionary<string, object>;
        var fromJsonElementOverload = JsonHelpers.ToPoco(jel) as Dictionary<string, object>;
        Assert.IsNotNull(fromObjectOverload);
        Assert.IsNotNull(fromJsonElementOverload);
        Assert.AreEqual(fromObjectOverload["Name"], fromJsonElementOverload["Name"]);
    }

    [TestMethod]
    public void ToPoco_UriString_DefaultBehavior_RemainsString()
    {
        var jel = Parse("\"https://example.com/path\"");
        var result = JsonHelpers.ToPoco(jel);
        Assert.IsInstanceOfType(result, typeof(string));
    }

    [TestMethod]
    public void ToPoco_UriString_WhenEnabled_ReturnsUri()
    {
        var settings = new JsonHelpers.ToPocoSettings { ConvertStringToUri = true };
        var jel = Parse("\"https://example.com/path\"");
        var result = JsonHelpers.ToPoco(jel, settings);
        Assert.IsInstanceOfType(result, typeof(Uri));
    }

    [TestMethod]
    public void ToPoco_DateOnlyString_WhenEnabled_ReturnsDateOnly()
    {
        var settings = new JsonHelpers.ToPocoSettings { ConvertStringToDateOnly = true, ConvertStringToDateTime = false, ConvertStringToDateTimeOffset = false };
        var jel = Parse("\"2026-02-14\"");
        var result = JsonHelpers.ToPoco(jel, settings);
        Assert.IsInstanceOfType(result, typeof(DateOnly));
    }

    [TestMethod]
    public void ToPoco_TimeOnlyString_WhenEnabled_ReturnsTimeOnly()
    {
        var settings = new JsonHelpers.ToPocoSettings { ConvertStringToTimeOnly = true };
        var jel = Parse("\"13:45:59\"");
        var result = JsonHelpers.ToPoco(jel, settings);
        Assert.IsInstanceOfType(result, typeof(TimeOnly));
    }

    [TestMethod]
    public void ToPoco_TimeSpanString_WhenEnabled_ReturnsTimeSpan()
    {
        var settings = new JsonHelpers.ToPocoSettings { ConvertStringToTimeSpan = true };
        var jel = Parse("\"01:02:03\"");
        var result = JsonHelpers.ToPoco(jel, settings);
        Assert.IsInstanceOfType(result, typeof(TimeSpan));
    }

    [TestMethod]
    public void ToPoco_VersionString_WhenEnabled_ReturnsVersion()
    {
        var settings = new JsonHelpers.ToPocoSettings { ConvertStringToVersion = true };
        var jel = Parse("\"1.2.3.4\"");
        var result = JsonHelpers.ToPoco(jel, settings);
        Assert.IsInstanceOfType(result, typeof(Version));
    }

    [TestMethod]
    public void ToPoco_Base64String_DefaultBehavior_RemainsString()
    {
        var text = "hello";
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
        var jel = Parse($"\"{base64}\"");
        var result = JsonHelpers.ToPoco(jel);
        Assert.IsInstanceOfType(result, typeof(string));
    }

    [TestMethod]
    public void ToPoco_Base64String_WhenEnabled_ReturnsBytes()
    {
        var expected = new byte[] { 1, 2, 3, 4, 5 };
        var base64 = Convert.ToBase64String(expected);
        var settings = new JsonHelpers.ToPocoSettings { ConvertStringToBytesFromBase64 = true };
        var jel = Parse($"\"{base64}\"");
        var result = JsonHelpers.ToPoco(jel, settings);
        Assert.IsInstanceOfType(result, typeof(byte[]));
        CollectionAssert.AreEqual(expected, (byte[]) result);
    }
}
