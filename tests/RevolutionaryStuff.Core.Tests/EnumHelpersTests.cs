using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class EnumHelpersTests
{
    [TestMethod]
    public void EnumRandomTest()
    {
        for (var z = 0; z < 100; ++z)
        {
            var a = EnumHelpers.Random<EItemTypes>();
            var b = EnumHelpers.Random<EItemTypes>();
            var c = EnumHelpers.Random<EItemTypes>();
            var d = EnumHelpers.Random<EItemTypes>();
            Assert.IsFalse(a == b && a == c && a == d);
        }
    }

    private static void CheckValue<TE>(string expected, TE raw) where TE : System.Enum
    {
        var ev = raw.EnumWithEnumMemberValuesToString();
        Assert.AreEqual(expected, ev);
    }

    [TestMethod]
    public void EnumWithEnumMemberValuesToStringTest()
    {
        CheckValue("a", EItemTypes.a);
        CheckValue("b", EItemTypes.b);
        CheckValue("DDD", EItemTypes.d);
        CheckValue("j", EItemTypes.j);
    }

    private enum EItemTypes
    {
        a = 1,
        b = 2,
        c = 3,
        [JsonStringEnumMemberName("DDD")]
        d = 4,
        e = 5,
        f = 6,
        g = 7,
        h = 8,
        i = 9,
        j = 10,
    }

    private enum EnumWithCustomNames
    {
        [EnumMember(Value = "legacy")]
        [JsonStringEnumMemberName("native")]
        LegacyAndJsonName,

        [JsonStringEnumMemberName("json-only")]
        JsonOnly,

        Unannotated
    }

    [TestMethod]
    public void EnumWithEnumMemberValuesToStringTest_PrefersLegacyValueAndSupportsJsonNames()
    {
        Assert.AreEqual("legacy", EnumWithCustomNames.LegacyAndJsonName.EnumWithEnumMemberValuesToString());
        Assert.AreEqual("json-only", EnumWithCustomNames.JsonOnly.EnumWithEnumMemberValuesToString());
        Assert.AreEqual(nameof(EnumWithCustomNames.Unannotated), EnumWithCustomNames.Unannotated.EnumWithEnumMemberValuesToString());
    }

    [TestMethod]
    public void ParseEnumWithEnumMemberValuesTest_SupportsLegacyAndJsonNames()
    {
        Assert.AreEqual(EnumWithCustomNames.LegacyAndJsonName, Parse.ParseEnumWithEnumMemberValues<EnumWithCustomNames>("legacy"));
        Assert.AreEqual(EnumWithCustomNames.LegacyAndJsonName, Parse.ParseEnumWithEnumMemberValues<EnumWithCustomNames>("native"));
        Assert.AreEqual(EnumWithCustomNames.JsonOnly, Parse.ParseEnumWithEnumMemberValues<EnumWithCustomNames>("json-only"));
        Assert.AreEqual(EnumWithCustomNames.Unannotated, Parse.ParseEnumWithEnumMemberValues<EnumWithCustomNames>(nameof(EnumWithCustomNames.Unannotated)));
    }
}
