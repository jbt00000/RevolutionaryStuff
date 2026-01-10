using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.ApiCore.Json;

namespace RevolutionaryStuff.ApiCore.Tests.Json;

[TestClass]
public class EnumMemberJsonConverterTests
{
    #region Test Enums

    public enum StatusWithoutAttribute
    {
        Active,
        Inactive,
        Pending
    }

    public enum StatusWithAttribute
    {
        [EnumMember(Value = "active")]
        Active,

        [EnumMember(Value = "inactive")]
        Inactive,

        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "archived")]
        Archived
    }

    public enum MixedAttributeStatus
    {
        [EnumMember(Value = "active")]
        Active,

        Inactive,  // No attribute

        [EnumMember(Value = "on-hold")]
        OnHold
    }

    public enum CaseSensitiveStatus
    {
        [EnumMember(Value = "ActiveStatus")]
        Active,

        [EnumMember(Value = "InactiveStatus")]
        Inactive
    }

    #endregion

    #region Test Models

    public class ModelWithEnum
    {
        public StatusWithAttribute Status { get; set; }
    }

    public class ModelWithDictionary
    {
        public Dictionary<StatusWithAttribute, int> StatusCounts { get; set; }
    }

    #endregion

    #region Read Tests

    [TestMethod]
    public void Read_WithEnumMemberAttribute_UsesAttributeValue()
    {
        // Arrange
        var json = """{"Status":"active"}""";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var result = JsonSerializer.Deserialize<ModelWithEnum>(json, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(StatusWithAttribute.Active, result.Status);
    }

    [TestMethod]
    public void Read_WithoutEnumMemberAttribute_UsesEnumName()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<StatusWithoutAttribute>();
        var json = "\"Active\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.Read(ref reader, typeof(StatusWithoutAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(StatusWithoutAttribute.Active, result);
    }

    [TestMethod]
    public void Read_CaseInsensitive_DefaultBehavior()
    {
        // Arrange - Default constructor uses _IgnoreCase = true
        var json = """{"Status":"ACTIVE"}""";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var result = JsonSerializer.Deserialize<ModelWithEnum>(json, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(StatusWithAttribute.Active, result.Status);
    }

    [TestMethod]
    public void Read_CaseSensitive_WhenConfigured()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<CaseSensitiveStatus>(_IgnoreCase: false);
        var json = "\"ActiveStatus\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.Read(ref reader, typeof(CaseSensitiveStatus), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(CaseSensitiveStatus.Active, result);
    }

    [TestMethod]
    public void Read_CaseSensitive_ThrowsOnMismatch()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<CaseSensitiveStatus>(_IgnoreCase: false);
        var json = "\"activestatus\"";  // Wrong case
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act & Assert
        try
        {
            converter.Read(ref reader, typeof(CaseSensitiveStatus), new JsonSerializerOptions());
            Assert.Fail("Expected JsonException was not thrown");
        }
        catch (JsonException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void Read_InvalidValue_ThrowsJsonException()
    {
        // Arrange
        var json = """{"Status":"invalid-status"}""";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act & Assert
        try
        {
            JsonSerializer.Deserialize<ModelWithEnum>(json, options);
            Assert.Fail("Expected JsonException was not thrown");
        }
        catch (JsonException ex)
        {
            Assert.IsTrue(ex.Message.Contains("Invalid value"));
        }
    }

    [TestMethod]
    public void Read_NullValue_ThrowsJsonException()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<StatusWithAttribute>();
        var json = "null";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act & Assert
        try
        {
            converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());
            Assert.Fail("Expected JsonException was not thrown");
        }
        catch (JsonException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void Read_AllAttributeValues_ParsesCorrectly()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<StatusWithAttribute>();
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

    [TestMethod]
    public void Read_EnumNameFallback_WhenNoAttributeMatch()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<MixedAttributeStatus>();
        var json = "\"Inactive\"";  // This one doesn't have an attribute
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.Read(ref reader, typeof(MixedAttributeStatus), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(MixedAttributeStatus.Inactive, result);
    }

    #endregion

    #region Write Tests

    [TestMethod]
    public void Write_WithEnumMemberAttribute_UsesAttributeValue()
    {
        // Arrange
        var model = new ModelWithEnum { Status = StatusWithAttribute.Active };
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.AreEqual("""{"Status":"active"}""", json);
    }

    [TestMethod]
    public void Write_WithoutEnumMemberAttribute_UsesEnumName()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<StatusWithoutAttribute>();
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
    public void Write_AllEnumValues_ProducesCorrectJson()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());
        var testCases = new[]
        {
            (StatusWithAttribute.Active, "active"),
            (StatusWithAttribute.Inactive, "inactive"),
            (StatusWithAttribute.Pending, "pending"),
            (StatusWithAttribute.Archived, "archived")
        };

        foreach (var (enumValue, expectedValue) in testCases)
        {
            var model = new ModelWithEnum { Status = enumValue };

            // Act
            var json = JsonSerializer.Serialize(model, options);

            // Assert
            var expectedJson = "{\"Status\":\"" + expectedValue + "\"}";
            Assert.AreEqual(expectedJson, json, $"Failed for {enumValue}");
        }
    }

    [TestMethod]
    public void Write_MixedAttributes_UsesAttributeOrName()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<MixedAttributeStatus>();
        
        var testCases = new[]
        {
            (MixedAttributeStatus.Active, "\"active\""),      // Has attribute
            (MixedAttributeStatus.Inactive, "\"Inactive\""),  // No attribute, uses name
            (MixedAttributeStatus.OnHold, "\"on-hold\"")      // Has attribute
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
        var json = """{"StatusCounts":{"active":10,"inactive":5}}""";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var result = JsonSerializer.Deserialize<ModelWithDictionary>(json, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.StatusCounts);
        Assert.AreEqual(2, result.StatusCounts.Count);
        Assert.AreEqual(10, result.StatusCounts[StatusWithAttribute.Active]);
        Assert.AreEqual(5, result.StatusCounts[StatusWithAttribute.Inactive]);
    }

    [TestMethod]
    public void ReadAsPropertyName_CaseInsensitive_Success()
    {
        // Arrange
        var json = """{"StatusCounts":{"ACTIVE":10}}""";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var result = JsonSerializer.Deserialize<ModelWithDictionary>(json, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.StatusCounts);
        Assert.AreEqual(10, result.StatusCounts[StatusWithAttribute.Active]);
    }

    [TestMethod]
    public void ReadAsPropertyName_InvalidPropertyName_ThrowsJsonException()
    {
        // Arrange
        var json = """{"StatusCounts":{"invalid-key":10}}""";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act & Assert
        try
        {
            JsonSerializer.Deserialize<ModelWithDictionary>(json, options);
            Assert.Fail("Expected JsonException was not thrown");
        }
        catch (JsonException ex)
        {
            Assert.IsTrue(ex.Message.Contains("Invalid property name"));
        }
    }

    [TestMethod]
    public void ReadAsPropertyName_EnumNameFallback_Success()
    {
        // Arrange
        var json = """{"StatusCounts":{"Active":10}}""";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var result = JsonSerializer.Deserialize<ModelWithDictionary>(json, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.StatusCounts);
        Assert.AreEqual(10, result.StatusCounts[StatusWithAttribute.Active]);
    }

    [TestMethod]
    public void ReadAsPropertyName_AllAttributeValues_ParsesCorrectly()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<StatusWithAttribute>();
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

    #endregion

    #region WriteAsPropertyName Tests

    [TestMethod]
    public void WriteAsPropertyName_WithEnumMemberAttribute_UsesAttributeValue()
    {
        // Arrange
        var model = new ModelWithDictionary
        {
            StatusCounts = new Dictionary<StatusWithAttribute, int>
            {
                { StatusWithAttribute.Active, 10 },
                { StatusWithAttribute.Inactive, 5 }
            }
        };
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.IsTrue(json.Contains("\"active\":10"));
        Assert.IsTrue(json.Contains("\"inactive\":5"));
    }

    [TestMethod]
    public void WriteAsPropertyName_AllEnumValues_ProducesCorrectJson()
    {
        // Arrange
        var model = new ModelWithDictionary
        {
            StatusCounts = new Dictionary<StatusWithAttribute, int>
            {
                { StatusWithAttribute.Active, 1 },
                { StatusWithAttribute.Inactive, 2 },
                { StatusWithAttribute.Pending, 3 },
                { StatusWithAttribute.Archived, 4 }
            }
        };
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.IsTrue(json.Contains("\"active\":1"));
        Assert.IsTrue(json.Contains("\"inactive\":2"));
        Assert.IsTrue(json.Contains("\"pending\":3"));
        Assert.IsTrue(json.Contains("\"archived\":4"));
    }

    [TestMethod]
    public void WriteAsPropertyName_WithoutAttribute_UsesEnumName()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<StatusWithoutAttribute>();
        var value = StatusWithoutAttribute.Pending;
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
        Assert.IsTrue(json.Contains("\"Pending\":123"));
    }

    #endregion

    #region Round-Trip Tests

    [TestMethod]
    public void RoundTrip_ValueSerialization_PreservesData()
    {
        // Arrange
        var original = new ModelWithEnum { Status = StatusWithAttribute.Pending };
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<ModelWithEnum>(json, options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Status, deserialized.Status);
    }

    [TestMethod]
    public void RoundTrip_DictionaryKeySerialization_PreservesData()
    {
        // Arrange
        var original = new ModelWithDictionary
        {
            StatusCounts = new Dictionary<StatusWithAttribute, int>
            {
                { StatusWithAttribute.Active, 100 },
                { StatusWithAttribute.Pending, 50 },
                { StatusWithAttribute.Archived, 25 }
            }
        };
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<ModelWithDictionary>(json, options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(deserialized.StatusCounts);
        Assert.AreEqual(3, deserialized.StatusCounts.Count);
        Assert.AreEqual(100, deserialized.StatusCounts[StatusWithAttribute.Active]);
        Assert.AreEqual(50, deserialized.StatusCounts[StatusWithAttribute.Pending]);
        Assert.AreEqual(25, deserialized.StatusCounts[StatusWithAttribute.Archived]);
    }

    [TestMethod]
    public void RoundTrip_PropertyName_PreservesData()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<StatusWithAttribute>();
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

    #region Constructor Parameter Tests

    [TestMethod]
    public void Constructor_DefaultIgnoreCase_IsTrue()
    {
        // Arrange & Act
        var converter = new EnumMemberJsonConverter<StatusWithAttribute>();
        var json = "\"ACTIVE\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Assert - Should not throw with different case
        var result = converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());
        Assert.AreEqual(StatusWithAttribute.Active, result);
    }

    [TestMethod]
    public void Constructor_IgnoreCaseFalse_EnforcesCaseSensitivity()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<StatusWithAttribute>(_IgnoreCase: false);
        var json = "\"ACTIVE\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act & Assert
        try
        {
            converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());
            Assert.Fail("Expected JsonException was not thrown");
        }
        catch (JsonException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void Constructor_IgnoreCaseFalse_ExactMatchWorks()
    {
        // Arrange
        var converter = new EnumMemberJsonConverter<StatusWithAttribute>(_IgnoreCase: false);
        var json = "\"active\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        // Act
        var result = converter.Read(ref reader, typeof(StatusWithAttribute), new JsonSerializerOptions());

        // Assert
        Assert.AreEqual(StatusWithAttribute.Active, result);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void Read_EmptyDictionary_Succeeds()
    {
        // Arrange
        var json = """{"StatusCounts":{}}""";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var result = JsonSerializer.Deserialize<ModelWithDictionary>(json, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.StatusCounts);
        Assert.AreEqual(0, result.StatusCounts.Count);
    }

    [TestMethod]
    public void Write_EmptyDictionary_ProducesEmptyObject()
    {
        // Arrange
        var model = new ModelWithDictionary
        {
            StatusCounts = new Dictionary<StatusWithAttribute, int>()
        };
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<StatusWithAttribute>());

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.IsTrue(json.Contains("\"StatusCounts\":{}"));
    }

    #endregion
}
