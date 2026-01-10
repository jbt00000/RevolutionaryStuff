using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft.Converters;

namespace RevolutionaryStuff.Core.Tests.Services.JsonSerializers;

[TestClass]
public class EnumMemberConverterTests
{
    #region Test Enums

    public enum StatusWithoutAttribute
    {
        Active = 1,
        Inactive = 2,
        Pending = 3
    }

    public enum StatusWithAttribute
    {
        [EnumMember(Value = "active")]
        Active = 1,

        [EnumMember(Value = "inactive")]
        Inactive = 2,

        [EnumMember(Value = "pending")]
        Pending = 3,

        [EnumMember(Value = "archived")]
        Archived = 4
    }

    public enum MixedAttributeStatus
    {
        [EnumMember(Value = "active")]
        Active = 10,

        Inactive = 20,  // No attribute

        [EnumMember(Value = "on-hold")]
        OnHold = 30
    }

    #endregion

    #region Test Models

    public class ModelWithEnum
    {
        [JsonConverter(typeof(EnumMemberConverter<StatusWithAttribute>))]
        public StatusWithAttribute Status { get; set; }
    }

    public class ModelWithDictionary
    {
        [JsonConverter(typeof(EnumMemberConverter<StatusWithAttribute>))]
        public Dictionary<StatusWithAttribute, int> StatusCounts { get; set; }
    }

    #endregion

    #region Read Tests - String Values

    [TestMethod]
    public void Read_StringWithEnumMemberAttribute_UsesAttributeValue()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var json = "\"active\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(StatusWithAttribute.Active, result);
    }

    [TestMethod]
    public void Read_StringWithoutEnumMemberAttribute_ParsesEnumName()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithoutAttribute>();
        var json = "\"Active\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.Read(ref reader, typeof(StatusWithoutAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(StatusWithoutAttribute.Active, result);
    }

    [TestMethod]
    public void Read_StringCaseVariations_HandlesCorrectly()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var testCases = new[] { "active", "ACTIVE", "Active", "AcTiVe" };

        foreach (var testCase in testCases)
        {
            var json = $"\"{testCase}\"";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            var result = converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

            // Assert
            Assert.AreEqual(StatusWithAttribute.Active, result, $"Failed for case: {testCase}");
        }
    }

    [TestMethod]
    public void Read_StringAllAttributeValues_ParsesCorrectly()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var testCases = new[]
        {
            ("active", StatusWithAttribute.Active),
            ("inactive", StatusWithAttribute.Inactive),
            ("pending", StatusWithAttribute.Pending),
            ("archived", StatusWithAttribute.Archived)
        };

        foreach (var (jsonValue, expectedEnum) in testCases)
        {
            var json = $"\"{jsonValue}\"";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            var result = converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

            // Assert
            Assert.AreEqual(expectedEnum, result, $"Failed for value: {jsonValue}");
        }
    }

    #endregion

    #region Read Tests - Numeric Values

    [TestMethod]
    public void Read_NumberValue_ParsesCorrectly()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var json = "1";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(StatusWithAttribute.Active, result);
    }

    [TestMethod]
    public void Read_NumberAllValues_ParsesCorrectly()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var testCases = new[]
        {
            (1, StatusWithAttribute.Active),
            (2, StatusWithAttribute.Inactive),
            (3, StatusWithAttribute.Pending),
            (4, StatusWithAttribute.Archived)
        };

        foreach (var (numValue, expectedEnum) in testCases)
        {
            var json = numValue.ToString();
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            var result = converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

            // Assert
            Assert.AreEqual(expectedEnum, result, $"Failed for number: {numValue}");
        }
    }

    [TestMethod]
    public void Read_NumberLargeValue_ParsesCorrectly()
    {
        // Arrange
        var converter = new EnumMemberConverter<MixedAttributeStatus>();
        var json = "30";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.Read(ref reader, typeof(MixedAttributeStatus), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(MixedAttributeStatus.OnHold, result);
    }

    #endregion

    #region Write Tests - SerializeEnumAsString = true

    [TestMethod]
    public void Write_AsString_WithEnumMemberAttribute_UsesAttributeValue()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = true };
        var value = StatusWithAttribute.Active;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, value, new JsonSerializerOptions());
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.AreEqual("\"active\"", json);
    }

    [TestMethod]
    public void Write_AsString_WithoutEnumMemberAttribute_UsesEnumName()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithoutAttribute> { SerializeEnumAsString = true };
        var value = StatusWithoutAttribute.Active;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, value, new JsonSerializerOptions());
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.AreEqual("\"Active\"", json);
    }

    [TestMethod]
    public void Write_AsString_AllEnumValues_ProducesCorrectJson()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = true };
        var testCases = new[]
        {
            (StatusWithAttribute.Active, "\"active\""),
            (StatusWithAttribute.Inactive, "\"inactive\""),
            (StatusWithAttribute.Pending, "\"pending\""),
            (StatusWithAttribute.Archived, "\"archived\"")
        };

        foreach (var (enumValue, expectedJson) in testCases)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            converter.Write(writer, enumValue, new JsonSerializerOptions());
            writer.Flush();
            var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

            // Assert
            Assert.AreEqual(expectedJson, json, $"Failed for {enumValue}");
        }
    }

    #endregion

    #region Write Tests - SerializeEnumAsString = false

    [TestMethod]
    public void Write_AsNumber_ProducesNumericValue()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = false };
        var value = StatusWithAttribute.Active;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, value, new JsonSerializerOptions());
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.AreEqual("1", json);
    }

    [TestMethod]
    public void Write_AsNumber_AllEnumValues_ProducesCorrectNumbers()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = false };
        var testCases = new[]
        {
            (StatusWithAttribute.Active, "1"),
            (StatusWithAttribute.Inactive, "2"),
            (StatusWithAttribute.Pending, "3"),
            (StatusWithAttribute.Archived, "4")
        };

        foreach (var (enumValue, expectedJson) in testCases)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            converter.Write(writer, enumValue, new JsonSerializerOptions());
            writer.Flush();
            var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

            // Assert
            Assert.AreEqual(expectedJson, json, $"Failed for {enumValue}");
        }
    }

    #endregion

    #region ReadAsPropertyName Tests

    [TestMethod]
    public void ReadAsPropertyName_WithEnumMemberAttribute_UsesAttributeValue()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var json = "\"active\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.ReadAsPropertyName(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(StatusWithAttribute.Active, result);
    }

    [TestMethod]
    public void ReadAsPropertyName_AllAttributeValues_ParsesCorrectly()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var testCases = new[]
        {
            ("active", StatusWithAttribute.Active),
            ("inactive", StatusWithAttribute.Inactive),
            ("pending", StatusWithAttribute.Pending),
            ("archived", StatusWithAttribute.Archived)
        };

        foreach (var (propertyName, expectedEnum) in testCases)
        {
            var json = $"\"{propertyName}\"";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            var result = converter.ReadAsPropertyName(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

            // Assert
            Assert.AreEqual(expectedEnum, result, $"Failed for property name: {propertyName}");
        }
    }

    [TestMethod]
    public void ReadAsPropertyName_EnumNameFallback_Success()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var json = "\"Active\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.ReadAsPropertyName(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(StatusWithAttribute.Active, result);
    }

    #endregion

    #region WriteAsPropertyName Tests

    [TestMethod]
    public void WriteAsPropertyName_WithEnumMemberAttribute_UsesAttributeValue()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var value = StatusWithAttribute.Active;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act - Property names must be written within an object
        writer.WriteStartObject();
        converter.WriteAsPropertyName(writer, value, new JsonSerializerOptions());
        writer.WriteNumberValue(123);
        writer.WriteEndObject();
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.IsTrue(json.Contains("\"active\":123"));
    }

    [TestMethod]
    public void WriteAsPropertyName_AllEnumValues_ProducesCorrectPropertyNames()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var testCases = new[]
        {
            (StatusWithAttribute.Active, "\"active\""),
            (StatusWithAttribute.Inactive, "\"inactive\""),
            (StatusWithAttribute.Pending, "\"pending\""),
            (StatusWithAttribute.Archived, "\"archived\"")
        };

        foreach (var (enumValue, expectedKey) in testCases)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            writer.WriteStartObject();
            converter.WriteAsPropertyName(writer, enumValue, new JsonSerializerOptions());
            writer.WriteNumberValue(999);
            writer.WriteEndObject();
            writer.Flush();
            var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

            // Assert
            Assert.IsTrue(json.Contains($"{expectedKey}:999"), $"Failed for {enumValue}. JSON: {json}");
        }
    }

    [TestMethod]
    public void WriteAsPropertyName_AlwaysUsesString_RegardlessOfSerializeEnumAsString()
    {
        // Arrange - SerializeEnumAsString = false should not affect property names
        var converter = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = false };
        var value = StatusWithAttribute.Active;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        writer.WriteStartObject();
        converter.WriteAsPropertyName(writer, value, new JsonSerializerOptions());
        writer.WriteNumberValue(456);
        writer.WriteEndObject();
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert - Should use string "active", not number "1"
        Assert.IsTrue(json.Contains("\"active\":456"));
        Assert.IsFalse(json.Contains("\"1\":"));
    }

    #endregion

    #region Round-Trip Tests

    [TestMethod]
    public void RoundTrip_StringSerialization_PreservesData()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = true };
        var original = StatusWithAttribute.Pending;

        using var writeStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(writeStream))
        {
            converter.Write(writer, original, new JsonSerializerOptions());
        }

        var json = writeStream.ToArray();
        var reader = new Utf8JsonReader(json);
        reader.Read();

        // Act
        var deserialized = converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void RoundTrip_NumericSerialization_PreservesData()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = false };
        var original = StatusWithAttribute.Inactive;

        using var writeStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(writeStream))
        {
            converter.Write(writer, original, new JsonSerializerOptions());
        }

        var json = writeStream.ToArray();
        var reader = new Utf8JsonReader(json);
        reader.Read();

        // Act
        var deserialized = converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(original, deserialized);
    }

    [TestMethod]
    public void RoundTrip_PropertyName_PreservesData()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute>();
        var original = StatusWithAttribute.Archived;

        // Write as property name
        using var writeStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(writeStream))
        {
            writer.WriteStartObject();
            converter.WriteAsPropertyName(writer, original, new JsonSerializerOptions());
            writer.WriteNumberValue(1);
            writer.WriteEndObject();
        }

        var json = System.Text.Encoding.UTF8.GetString(writeStream.ToArray());
        
        // Parse the JSON to extract the property name
        using var doc = JsonDocument.Parse(json);
        var propertyName = doc.RootElement.EnumerateObject().First().Name;
        
        // Read the property name back
        var propertyNameBytes = System.Text.Encoding.UTF8.GetBytes($"\"{propertyName}\"");
        var reader = new Utf8JsonReader(propertyNameBytes);
        reader.Read();

        // Act
        var deserialized = converter.ReadAsPropertyName(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(original, deserialized);
    }

    #endregion

    #region SerializeEnumAsString Property Tests

    [TestMethod]
    public void SerializeEnumAsString_DefaultValue_IsTrue()
    {
        // Arrange & Act
        var converter = new EnumMemberConverter<StatusWithAttribute>();

        // Assert
        Assert.IsTrue(converter.SerializeEnumAsString);
    }

    [TestMethod]
    public void SerializeEnumAsString_CanBeSetToFalse()
    {
        // Arrange
        var converter = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = false };

        // Assert
        Assert.IsFalse(converter.SerializeEnumAsString);
    }

    [TestMethod]
    public void SerializeEnumAsString_AffectsWriteBehavior()
    {
        // Arrange
        var value = StatusWithAttribute.Active;

        // Test with SerializeEnumAsString = true
        var converterString = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = true };
        using var streamString = new MemoryStream();
        using (var writerString = new Utf8JsonWriter(streamString))
        {
            converterString.Write(writerString, value, new JsonSerializerOptions());
        }
        var jsonString = System.Text.Encoding.UTF8.GetString(streamString.ToArray());

        // Test with SerializeEnumAsString = false
        var converterNumber = new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = false };
        using var streamNumber = new MemoryStream();
        using (var writerNumber = new Utf8JsonWriter(streamNumber))
        {
            converterNumber.Write(writerNumber, value, new JsonSerializerOptions());
        }
        var jsonNumber = System.Text.Encoding.UTF8.GetString(streamNumber.ToArray());

        // Assert
        Assert.AreEqual("\"active\"", jsonString);
        Assert.AreEqual("1", jsonNumber);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void Integration_FullObjectSerialization_WithStringValues()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = true });
        
        // Use a simple object without JsonConverter attribute to ensure our converter is used
        var model = new { Status = StatusWithAttribute.Pending };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.IsTrue(json.Contains("\"pending\""));
    }

    [TestMethod]
    public void Integration_FullObjectSerialization_WithNumericValues()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberConverter<StatusWithAttribute> { SerializeEnumAsString = false });
        
        // Use a simple object without JsonConverter attribute
        var model = new { Status = StatusWithAttribute.Inactive };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert - When serialized as number, it will be: {"Status":2} not {"Status":"2"}
        Assert.IsTrue(json.Contains(":2"), $"JSON was: {json}");
    }

    #endregion
}
