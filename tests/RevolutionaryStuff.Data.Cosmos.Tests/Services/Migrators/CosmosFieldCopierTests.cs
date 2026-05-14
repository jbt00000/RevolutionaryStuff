using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Data.Cosmos.Services.Migrators;

namespace RevolutionaryStuff.Data.Cosmos.Tests;

[TestClass]
public class CosmosFieldCopierTests
{
    // -----------------------------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// Invokes the private static CopyField method on <see cref="CosmosFieldCopier"/> so we can
    /// unit-test the JSON transformation without a live Cosmos container.
    /// </summary>
    private static string CopyField(string json, string sourceFieldName, string destFieldName)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty(sourceFieldName, out var sourceValue);

        var method = typeof(CosmosFieldCopier).GetMethod(
            "CopyField",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsNotNull(method, "CopyField static method not found via reflection");

        var resultBytes = (byte[])method.Invoke(null, [root, sourceFieldName, destFieldName, sourceValue])!;
        return Encoding.UTF8.GetString(resultBytes);
    }

    private static void AssertJsonEqual(string expected, string actual, string message = "")
    {
        var expectedNorm = JsonSerializer.Serialize(JsonDocument.Parse(expected), new JsonSerializerOptions { WriteIndented = false });
        var actualNorm   = JsonSerializer.Serialize(JsonDocument.Parse(actual),   new JsonSerializerOptions { WriteIndented = false });
        Assert.AreEqual(expectedNorm, actualNorm, message);
    }

    // -----------------------------------------------------------------------------------------
    // CopyField tests
    // -----------------------------------------------------------------------------------------

    [TestMethod]
    public void CopyField_DestAbsentSourcePresent_DestAddedAfterSource()
    {
        var json   = """{"id":"1","source":"hello","other":"world"}""";
        var result = CopyField(json, "source", "dest");

        var doc = JsonDocument.Parse(result);
        Assert.IsTrue(doc.RootElement.TryGetProperty("dest", out var dest), "dest field should be present");
        Assert.AreEqual("hello", dest.GetString());
    }

    [TestMethod]
    public void CopyField_DestAbsent_SourceStringValue_CopiedCorrectly()
    {
        var json   = """{"id":"1","mySource":"value123"}""";
        var result = CopyField(json, "mySource", "myDest");

        var doc = JsonDocument.Parse(result);
        Assert.IsTrue(doc.RootElement.TryGetProperty("myDest", out var dest));
        Assert.AreEqual("value123", dest.GetString());
    }

    [TestMethod]
    public void CopyField_DestAbsent_SourceIntValue_CopiedCorrectly()
    {
        var json   = """{"id":"1","count":42}""";
        var result = CopyField(json, "count", "countCopy");

        var doc = JsonDocument.Parse(result);
        Assert.IsTrue(doc.RootElement.TryGetProperty("countCopy", out var dest));
        Assert.AreEqual(42, dest.GetInt32());
    }

    [TestMethod]
    public void CopyField_DestAbsent_SourceBoolValue_CopiedCorrectly()
    {
        var json   = """{"id":"1","flag":true}""";
        var result = CopyField(json, "flag", "flagCopy");

        var doc = JsonDocument.Parse(result);
        Assert.IsTrue(doc.RootElement.TryGetProperty("flagCopy", out var dest));
        Assert.IsTrue(dest.GetBoolean());
    }

    [TestMethod]
    public void CopyField_DestAbsent_SourceNullValue_CopiedCorrectly()
    {
        var json   = """{"id":"1","nullable":null}""";
        var result = CopyField(json, "nullable", "nullableCopy");

        var doc = JsonDocument.Parse(result);
        Assert.IsTrue(doc.RootElement.TryGetProperty("nullableCopy", out var dest));
        Assert.AreEqual(JsonValueKind.Null, dest.ValueKind);
    }

    [TestMethod]
    public void CopyField_DestInsertedImmediatelyAfterSource()
    {
        var json   = """{"a":"1","source":"v","b":"2"}""";
        var result = CopyField(json, "source", "dest");

        // Check ordering: dest should come right after source
        var doc       = JsonDocument.Parse(result);
        var propNames = doc.RootElement.EnumerateObject().Select(p => p.Name).ToList();
        var srcIdx    = propNames.IndexOf("source");
        var dstIdx    = propNames.IndexOf("dest");

        Assert.IsTrue(srcIdx >= 0, "source should be present");
        Assert.AreEqual(srcIdx + 1, dstIdx, "dest should be inserted immediately after source");
    }

    [TestMethod]
    public void CopyField_OtherPropertiesPreserved()
    {
        var json   = """{"id":"1","source":"hello","other":"world","num":99}""";
        var result = CopyField(json, "source", "dest");

        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        Assert.AreEqual("1",     root.GetProperty("id").GetString());
        Assert.AreEqual("hello", root.GetProperty("source").GetString());
        Assert.AreEqual("world", root.GetProperty("other").GetString());
        Assert.AreEqual(99,      root.GetProperty("num").GetInt32());
    }

    [TestMethod]
    public void CopyField_SourceObjectValue_CopiedByValue()
    {
        var json   = """{"id":"1","nested":{"x":1,"y":2}}""";
        var result = CopyField(json, "nested", "nestedCopy");

        var doc = JsonDocument.Parse(result);
        Assert.IsTrue(doc.RootElement.TryGetProperty("nestedCopy", out var dest));
        Assert.AreEqual(JsonValueKind.Object, dest.ValueKind);
        Assert.AreEqual(1, dest.GetProperty("x").GetInt32());
        Assert.AreEqual(2, dest.GetProperty("y").GetInt32());
    }

    [TestMethod]
    public void CopyField_SourceArrayValue_CopiedByValue()
    {
        var json   = """{"id":"1","tags":["a","b","c"]}""";
        var result = CopyField(json, "tags", "tagsCopy");

        var doc = JsonDocument.Parse(result);
        Assert.IsTrue(doc.RootElement.TryGetProperty("tagsCopy", out var dest));
        Assert.AreEqual(JsonValueKind.Array, dest.ValueKind);
        var items = dest.EnumerateArray().Select(e => e.GetString()).ToList();
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, items);
    }

    // -----------------------------------------------------------------------------------------
    // Config / construction guard tests
    // -----------------------------------------------------------------------------------------

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_NullConfig_Throws()
    {
        _ = new CosmosFieldCopier(
            null!,
            new RevolutionaryStuff.Core.RevolutionaryStuffService.RevolutionaryStuffServiceConstrutorArgs(null!));
    }
}
