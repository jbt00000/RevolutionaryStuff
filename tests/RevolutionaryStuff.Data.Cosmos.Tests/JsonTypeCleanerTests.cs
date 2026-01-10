using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Data.Cosmos;

namespace RevolutionaryStuff.Data.Cosmos.Tests;

[TestClass]
public class JsonTypeCleanerTests
{
    private const string DefaultPropertyName = "$type";

    private static string ReadStream(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static void AssertJsonEqual(string expected, string actual, string message = "")
    {
        // Normalize by parsing and re-serializing both to remove whitespace differences
        var expectedDoc = JsonDocument.Parse(expected);
        var actualDoc = JsonDocument.Parse(actual);
        
        var expectedNormalized = JsonSerializer.Serialize(expectedDoc, new JsonSerializerOptions { WriteIndented = false });
        var actualNormalized = JsonSerializer.Serialize(actualDoc, new JsonSerializerOptions { WriteIndented = false });
        
        Assert.AreEqual(expectedNormalized, actualNormalized, message);
    }

    #region Simple/Primitive Types Tests

    [TestMethod]
    public void RemoveDuplicateTypeKeys_SimpleString_ReturnsOriginal()
    {
        var json = "\"hello world\"";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Simple string should be returned as-is");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_SimpleNumber_ReturnsOriginal()
    {
        var json = "42";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Simple number should be returned as-is");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_SimpleDecimal_ReturnsOriginal()
    {
        var json = "3.14159";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Simple decimal should be returned as-is");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_SimpleBoolean_ReturnsOriginal()
    {
        var json = "true";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Simple boolean should be returned as-is");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_SimpleNull_ReturnsOriginal()
    {
        var json = "null";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Simple null should be returned as-is");
    }

    #endregion

    #region Array Tests

    [TestMethod]
    public void RemoveDuplicateTypeKeys_EmptyArray_ReturnsOriginal()
    {
        var json = "[]";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Empty array should be returned as-is");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_SimpleArray_ReturnsOriginal()
    {
        var json = "[1, 2, 3, \"test\", true, null]";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Simple array should be returned as-is");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_ArrayOfObjects_RemovesDuplicates()
    {
        var json = @"[
            {""$type"": ""Type1"", ""name"": ""first"", ""$type"": ""Type2""},
            {""$type"": ""Type3"", ""name"": ""second""}
        ]";
        var expected = @"[
            {""$type"": ""Type1"", ""name"": ""first""},
            {""$type"": ""Type3"", ""name"": ""second""}
        ]";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should remove duplicate $type from objects in array");
    }

    #endregion

    #region Object Tests

    [TestMethod]
    public void RemoveDuplicateTypeKeys_EmptyObject_ReturnsOriginal()
    {
        var json = "{}";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Empty object should be returned as-is");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_ObjectWithoutTypeProperty_ReturnsOriginal()
    {
        var json = @"{""name"": ""John"", ""age"": 30, ""active"": true}";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Object without $type should be returned as-is");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_ObjectWithSingleTypeProperty_ReturnsOriginal()
    {
        var json = @"{""$type"": ""Person"", ""name"": ""John"", ""age"": 30}";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(json, result, "Object with single $type should be returned as-is");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_ObjectWithDuplicateTypeAtEnd_RemovesSecond()
    {
        var json = @"{""$type"": ""Person"", ""name"": ""John"", ""$type"": ""Employee""}";
        var expected = @"{""$type"": ""Person"", ""name"": ""John""}";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should remove second $type property");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_ObjectWithMultipleDuplicates_KeepsOnlyFirst()
    {
        var json = @"{""$type"": ""Type1"", ""name"": ""John"", ""$type"": ""Type2"", ""age"": 30, ""$type"": ""Type3""}";
        var expected = @"{""$type"": ""Type1"", ""name"": ""John"", ""age"": 30}";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should keep only first $type and remove all others");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_ObjectWithDuplicateTypeAtStart_RemovesSecond()
    {
        var json = @"{""$type"": ""Person"", ""$type"": ""Employee"", ""name"": ""John""}";
        var expected = @"{""$type"": ""Person"", ""name"": ""John""}";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should remove second $type when consecutive");
    }

    #endregion

    #region Nested Object Tests

    [TestMethod]
    public void RemoveDuplicateTypeKeys_NestedObjects_RemovesDuplicatesAtEachLevel()
    {
        var json = @"{
            ""$type"": ""Parent"",
            ""child"": {
                ""$type"": ""Child"",
                ""name"": ""nested"",
                ""$type"": ""ChildDupe""
            },
            ""$type"": ""ParentDupe""
        }";
        var expected = @"{
            ""$type"": ""Parent"",
            ""child"": {
                ""$type"": ""Child"",
                ""name"": ""nested""
            }
        }";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should remove duplicates at each nesting level independently");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_DeeplyNestedObjects_HandlesAllLevels()
    {
        var json = @"{
            ""$type"": ""L1"",
            ""level1"": {
                ""$type"": ""L2"",
                ""level2"": {
                    ""$type"": ""L3"",
                    ""level3"": {
                        ""$type"": ""L4"",
                        ""value"": 42,
                        ""$type"": ""L4Dupe""
                    },
                    ""$type"": ""L3Dupe""
                },
                ""$type"": ""L2Dupe""
            },
            ""$type"": ""L1Dupe""
        }";
        var expected = @"{
            ""$type"": ""L1"",
            ""level1"": {
                ""$type"": ""L2"",
                ""level2"": {
                    ""$type"": ""L3"",
                    ""level3"": {
                        ""$type"": ""L4"",
                        ""value"": 42
                    }
                }
            }
        }";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should handle deeply nested objects correctly");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_ObjectWithNestedArray_HandlesCorrectly()
    {
        var json = @"{
            ""$type"": ""Container"",
            ""items"": [
                {""$type"": ""Item1"", ""name"": ""first"", ""$type"": ""Item1Dupe""},
                {""$type"": ""Item2"", ""name"": ""second""}
            ],
            ""$type"": ""ContainerDupe""
        }";
        var expected = @"{
            ""$type"": ""Container"",
            ""items"": [
                {""$type"": ""Item1"", ""name"": ""first""},
                {""$type"": ""Item2"", ""name"": ""second""}
            ]
        }";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should handle objects within arrays correctly");
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void RemoveDuplicateTypeKeys_ObjectWithOtherDuplicateProperties_OnlyRemovesTypeProperty()
    {
        var json = @"{
            ""$type"": ""Person"",
            ""name"": ""John"",
            ""name"": ""Jane"",
            ""$type"": ""Employee"",
            ""age"": 30
        }";
        var expected = @"{
            ""$type"": ""Person"",
            ""name"": ""John"",
            ""name"": ""Jane"",
            ""age"": 30
        }";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should only remove $type duplicates, not other properties");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_CustomPropertyName_RemovesCustomDuplicates()
    {
        var json = @"{""@class"": ""Type1"", ""name"": ""John"", ""@class"": ""Type2""}";
        var expected = @"{""@class"": ""Type1"", ""name"": ""John""}";
        var result = CallRemoveDuplicateTypeKeys(json, "@class");
        AssertJsonEqual(expected, result, "Should work with custom property names");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_EmptyPropertyName_ThrowsOrHandlesGracefully()
    {
        var json = @"{""$type"": ""Person"", ""name"": ""John""}";
        // This should either throw or return original - testing for graceful handling
        var result = CallRemoveDuplicateTypeKeys(json, "");
        AssertJsonEqual(json, result, "Empty property name should return original JSON");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_ComplexMixedStructure_HandlesCorrectly()
    {
        var json = @"{
            ""$type"": ""Root"",
            ""primitives"": [1, 2, ""three"", true, null],
            ""objects"": [
                {""$type"": ""Obj1"", ""data"": ""value1"", ""$type"": ""Obj1Dupe""},
                {""$type"": ""Obj2"", ""data"": ""value2""}
            ],
            ""nested"": {
                ""$type"": ""Nested"",
                ""array"": [
                    {""$type"": ""Item"", ""x"": 1, ""$type"": ""ItemDupe""}
                ],
                ""$type"": ""NestedDupe""
            },
            ""$type"": ""RootDupe""
        }";
        var expected = @"{
            ""$type"": ""Root"",
            ""primitives"": [1, 2, ""three"", true, null],
            ""objects"": [
                {""$type"": ""Obj1"", ""data"": ""value1""},
                {""$type"": ""Obj2"", ""data"": ""value2""}
            ],
            ""nested"": {
                ""$type"": ""Nested"",
                ""array"": [
                    {""$type"": ""Item"", ""x"": 1}
                ]
            }
        }";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should handle complex mixed structures correctly");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_LargeNumbers_PreservesExactValues()
    {
        var json = @"{
            ""$type"": ""Numbers"",
            ""bigInt"": 9223372036854775807,
            ""decimal"": 123.456789012345,
            ""scientific"": 1.23E-10,
            ""$type"": ""NumbersDupe""
        }";
        var expected = @"{
            ""$type"": ""Numbers"",
            ""bigInt"": 9223372036854775807,
            ""decimal"": 123.456789012345,
            ""scientific"": 1.23E-10
        }";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should preserve exact numeric values");
    }

    [TestMethod]
    public void RemoveDuplicateTypeKeys_SpecialCharactersInStrings_PreservesCorrectly()
    {
        var json = @"{
            ""$type"": ""Special"",
            ""text"": ""Line1\nLine2\tTabbed"",
            ""unicode"": ""Hello 世界 🌍"",
            ""$type"": ""SpecialDupe""
        }";
        var expected = @"{
            ""$type"": ""Special"",
            ""text"": ""Line1\nLine2\tTabbed"",
            ""unicode"": ""Hello 世界 🌍""
        }";
        var result = CallRemoveDuplicateTypeKeys(json);
        AssertJsonEqual(expected, result, "Should preserve special characters correctly");
    }

    #endregion

    #region Helper Methods

    private static string CallRemoveDuplicateTypeKeys(string json, string propertyName = DefaultPropertyName)
    {
        // Use reflection to access the private nested class method
        var adaptorType = typeof(JsonSerializer2CosmosSerializerAdaptor);
        var cleanerType = adaptorType.GetNestedType("JsonTypeCleaner", System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(cleanerType, "JsonTypeCleaner nested class not found");

        var method = cleanerType.GetMethod("RemoveDuplicateTypeKeys", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        Assert.IsNotNull(method, "RemoveDuplicateTypeKeys method not found");

        var stream = (Stream)method.Invoke(null, new object[] { json, propertyName });
        return ReadStream(stream);
    }

    #endregion
}
