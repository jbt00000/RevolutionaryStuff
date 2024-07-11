using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class SystemTextJsonTests
{
    public class Simple : JsonSerializable
    {
        public string A { get; set; } = "A";
        public int B { get; set; }
    }

    public class SimpleWithJsonElement : Simple
    {
        public JsonElement Jel { get; set; }
    }

    [TestMethod]
    public void WriteJelWithValueKindUndefined()
    {
        var val = new SimpleWithJsonElement();
        var json = val.ToJson();
        Assert.IsTrue(json.Contains(nameof(SimpleWithJsonElement.Jel)));
        Assert.IsTrue(json.Contains("null"));
    }

    [TestMethod]
    public void WriteJelWithValueKindInt()
    {
        var val = new SimpleWithJsonElement()
        {
            Jel = JsonDocument.Parse("42").RootElement
        };
        var json = val.ToJson();
        Assert.IsTrue(json.Contains(nameof(SimpleWithJsonElement.Jel)));
        Assert.IsTrue(json.Contains("42"));
    }

    [TestMethod]
    public void WriteJelWithValueKindJson()
    {
        var val = new SimpleWithJsonElement()
        {
            Jel = JsonDocument.Parse("""
{
    "a": "1",
    "c": "3",
    "zippy": {
        "d": "4",
        "e": "5"
    }
}

""").RootElement
        };
        var json = val.ToJson();
        Assert.IsTrue(json.Contains(nameof(SimpleWithJsonElement.Jel)));
        Assert.IsTrue(json.Contains("42"));
    }
}
