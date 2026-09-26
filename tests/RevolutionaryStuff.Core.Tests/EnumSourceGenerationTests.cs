using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

internal enum EnumJsonSourceGenerationTestEnum
{
    [JsonStringEnumMemberName("custom-value")]
    CustomValue
}

internal class EnumJsonSourceGenerationTestModel
{
    public EnumJsonSourceGenerationTestEnum Value { get; set; }
    public EnumJsonSourceGenerationTestEnum? OptionalValue { get; set; }
    public Dictionary<EnumJsonSourceGenerationTestEnum, int> Counts { get; set; } = [];
}

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(EnumJsonSourceGenerationTestModel))]
internal partial class EnumJsonSourceGenerationTestContext : JsonSerializerContext
{ }

[TestClass]
public class EnumSourceGenerationTests
{
    [TestMethod]
    public void SourceGeneratedJson_UsesCustomEnumMemberName()
    {
        var value = new EnumJsonSourceGenerationTestModel { Value = EnumJsonSourceGenerationTestEnum.CustomValue };

        var json = JsonSerializer.Serialize(value, EnumJsonSourceGenerationTestContext.Default.EnumJsonSourceGenerationTestModel);
        var deserialized = JsonSerializer.Deserialize(json, EnumJsonSourceGenerationTestContext.Default.EnumJsonSourceGenerationTestModel);
        using var jsonDocument = JsonDocument.Parse(json);

        Assert.AreEqual("custom-value", jsonDocument.RootElement.GetProperty(nameof(value.Value)).GetString());
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(value.Value, deserialized.Value);
    }

    [TestMethod]
    public void DefaultSerializer_UsesCustomNamesForNullableValuesAndDictionaryKeys()
    {
        var value = new EnumJsonSourceGenerationTestModel
        {
            Value = EnumJsonSourceGenerationTestEnum.CustomValue,
            OptionalValue = EnumJsonSourceGenerationTestEnum.CustomValue,
            Counts = new() { [EnumJsonSourceGenerationTestEnum.CustomValue] = 1 }
        };

        var json = JsonHelpers.ToMicrosoftJson(value);
        var deserialized = JsonHelpers.FromMicrosoftJson<EnumJsonSourceGenerationTestModel>(json);
        using var jsonDocument = JsonDocument.Parse(json);

        Assert.AreEqual("custom-value", jsonDocument.RootElement.GetProperty(nameof(value.Value)).GetString());
        Assert.AreEqual("custom-value", jsonDocument.RootElement.GetProperty(nameof(value.OptionalValue)).GetString());
        Assert.AreEqual(1, jsonDocument.RootElement.GetProperty(nameof(value.Counts)).GetProperty("custom-value").GetInt32());
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(value.Value, deserialized.Value);
        Assert.AreEqual(value.OptionalValue, deserialized.OptionalValue);
        Assert.AreEqual(1, deserialized.Counts[EnumJsonSourceGenerationTestEnum.CustomValue]);
    }
}
