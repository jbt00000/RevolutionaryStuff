using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class TypeHelpersTests
{
    #region Existing Convert Tests

    [TestMethod]
    public void ConvertFromUriString()
        => Assert.AreEqual(new Uri("http://www.espn.com"), TypeHelpers.ConvertValue(typeof(Uri), "http://www.espn.com"));

    [TestMethod]
    public void ConvertFromBoolTrueString()
        => Assert.AreEqual(true, TypeHelpers.ConvertValue(typeof(bool), "true"));

    [TestMethod]
    public void ConvertFromBool1String()
        => Assert.AreEqual(true, TypeHelpers.ConvertValue(typeof(bool), "1"));

    [TestMethod]
    public void ConvertFromBoolFalseString()
        => Assert.AreEqual(false, TypeHelpers.ConvertValue(typeof(bool), "false"));

    [TestMethod]
    public void ConvertFromBool0String()
        => Assert.AreEqual(false, TypeHelpers.ConvertValue(typeof(bool), "0"));

    public enum ConvertTestEnum
    {
        a = 1,
        b = 2,
        c = 3
    }

    [TestMethod]
    public void ConvertFromEnumString()
        => Assert.AreEqual(ConvertTestEnum.b, TypeHelpers.ConvertValue(typeof(ConvertTestEnum), "b"));

    [TestMethod]
    public void ConvertFromEnumWrongCaseString()
        => Assert.AreEqual(ConvertTestEnum.b, TypeHelpers.ConvertValue(typeof(ConvertTestEnum), "B"));

    [TestMethod]
    public void ConvertFromEnumNumberString()
        => Assert.AreEqual(ConvertTestEnum.b, TypeHelpers.ConvertValue(typeof(ConvertTestEnum), "2"));

    [TestMethod]
    public void ConvertFromNumber5String()
        => Assert.AreEqual(5, TypeHelpers.ConvertValue(typeof(int), "5"));

    #endregion

    #region IsValueTypeOrString Tests

    [TestMethod]
    public void IsValueTypeOrString_WithInt_ReturnsTrue()
        => Assert.IsTrue(typeof(int).IsValueTypeOrString());

    [TestMethod]
    public void IsValueTypeOrString_WithString_ReturnsTrue()
        => Assert.IsTrue(typeof(string).IsValueTypeOrString());

    [TestMethod]
    public void IsValueTypeOrString_WithStruct_ReturnsTrue()
        => Assert.IsTrue(typeof(DateTime).IsValueTypeOrString());

    [TestMethod]
    public void IsValueTypeOrString_WithClass_ReturnsFalse()
        => Assert.IsFalse(typeof(List<int>).IsValueTypeOrString());

    #endregion

    #region IsNullableEnum Tests

    [TestMethod]
    public void IsNullableEnum_WithNullableEnum_ReturnsTrue()
        => Assert.IsTrue(typeof(DayOfWeek?).IsNullableEnum());

    [TestMethod]
    public void IsNullableEnum_WithEnum_ReturnsFalse()
        => Assert.IsFalse(typeof(DayOfWeek).IsNullableEnum());

    [TestMethod]
    public void IsNullableEnum_WithNullableInt_ReturnsFalse()
        => Assert.IsFalse(typeof(int?).IsNullableEnum());

    [TestMethod]
    public void IsNullableEnum_WithReferenceType_ReturnsFalse()
        => Assert.IsFalse(typeof(string).IsNullableEnum());

    #endregion

    #region IsNullable Tests

    [TestMethod]
    public void IsNullable_WithReferenceType_ReturnsTrue()
        => Assert.IsTrue(typeof(string).IsNullable());

    [TestMethod]
    public void IsNullable_WithNullableValueType_ReturnsTrue()
        => Assert.IsTrue(typeof(int?).IsNullable());

    [TestMethod]
    public void IsNullable_WithValueType_ReturnsFalse()
        => Assert.IsFalse(typeof(int).IsNullable());

    #endregion

    #region GetDefaultValue Tests

    [TestMethod]
    public void GetDefaultValue_WithInt_ReturnsZero()
    {
        var result = typeof(int).GetDefaultValue();
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void GetDefaultValue_WithString_ReturnsNull()
    {
        var result = typeof(string).GetDefaultValue();
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetDefaultValue_WithDateTime_ReturnsMinValue()
    {
        var result = typeof(DateTime).GetDefaultValue();
        Assert.AreEqual(default(DateTime), result);
    }

    #endregion

    #region Construct Tests

    [TestMethod]
    public void Construct_WithSimpleClass_CreatesInstance()
    {
        var result = typeof(List<int>).Construct();
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<List<int>>(result);
    }

    [TestMethod]
    public void Construct_WithIListInterface_CreatesListInstance()
    {
        var result = typeof(IList<int>).Construct();
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<IList>(result);
    }

    [TestMethod]
    public void Construct_WithIDictionaryInterface_CreatesDictionaryInstance()
    {
        var result = typeof(IDictionary<string, int>).Construct();
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<IDictionary>(result);
    }

    [TestMethod]
    public void ConstructGeneric_CreatesInstance()
    {
        var result = TypeHelpers.Construct<List<string>>();
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<List<string>>(result);
    }

    [TestMethod]
    public void ConstructDictionary_CreatesCorrectType()
    {
        var result = TypeHelpers.ConstructDictionary(typeof(string), typeof(int));
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<Dictionary<string, int>>(result);
    }

    [TestMethod]
    public void ConstructList_CreatesCorrectType()
    {
        var result = TypeHelpers.ConstructList(typeof(string));
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<List<string>>(result);
    }

    #endregion

    #region GetIndexer Tests

    [TestMethod]
    public void GetIndexer_WithListType_ReturnsIndexer()
    {
        var indexer = typeof(List<int>).GetIndexer();
        Assert.IsNotNull(indexer);
        Assert.AreEqual("Item", indexer.Name);
    }

    [TestMethod]
    public void GetIndexer_WithNonIndexedType_ReturnsNull()
    {
        var indexer = typeof(int).GetIndexer();
        Assert.IsNull(indexer);
    }

    #endregion

    #region ToPropertyValueDictionary Tests

    [TestMethod]
    public void ToPropertyValueDictionary_WithNull_ReturnsEmptyDictionary()
    {
        var result = TypeHelpers.ToPropertyValueDictionary(null);
        Assert.IsNotNull(result);
        Assert.HasCount(0, result);
    }

    [TestMethod]
    public void ToPropertyValueDictionary_WithSimpleObject_ReturnsDictionary()
    {
        var obj = new { Name = "Test", Age = 30 };
        var result = TypeHelpers.ToPropertyValueDictionary(obj);
        Assert.HasCount(2, result);
        Assert.AreEqual("Test", result["Name"]);
        Assert.AreEqual(30, result["Age"]);
    }

    [TestMethod]
    public void ToPropertyValueDictionary_WithExpandoObject_ReturnsSameInstance()
    {
        dynamic expando = new ExpandoObject();
        expando.Name = "Test";
        var result = TypeHelpers.ToPropertyValueDictionary(expando);
        Assert.AreSame(expando, result);
    }

    #endregion

    #region IsWholeNumber Tests - .NET 9 Support

    [TestMethod]
    public void IsWholeNumber_WithByte_ReturnsTrue()
        => Assert.IsTrue(typeof(byte).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithSByte_ReturnsTrue()
        => Assert.IsTrue(typeof(sbyte).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithShort_ReturnsTrue()
        => Assert.IsTrue(typeof(short).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithUShort_ReturnsTrue()
        => Assert.IsTrue(typeof(ushort).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithInt_ReturnsTrue()
        => Assert.IsTrue(typeof(int).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithUInt_ReturnsTrue()
        => Assert.IsTrue(typeof(uint).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithLong_ReturnsTrue()
        => Assert.IsTrue(typeof(long).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithULong_ReturnsTrue()
        => Assert.IsTrue(typeof(ulong).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithNInt_ReturnsTrue()
        => Assert.IsTrue(typeof(nint).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithNUInt_ReturnsTrue()
        => Assert.IsTrue(typeof(nuint).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithInt128_ReturnsTrue()
        => Assert.IsTrue(typeof(Int128).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithUInt128_ReturnsTrue()
        => Assert.IsTrue(typeof(UInt128).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithFloat_ReturnsFalse()
        => Assert.IsFalse(typeof(float).IsWholeNumber());

    [TestMethod]
    public void IsWholeNumber_WithString_ReturnsFalse()
        => Assert.IsFalse(typeof(string).IsWholeNumber());

    #endregion

    #region IsRealNumber Tests - .NET 9 Support

    [TestMethod]
    public void IsRealNumber_WithFloat_ReturnsTrue()
        => Assert.IsTrue(typeof(float).IsRealNumber());

    [TestMethod]
    public void IsRealNumber_WithDouble_ReturnsTrue()
        => Assert.IsTrue(typeof(double).IsRealNumber());

    [TestMethod]
    public void IsRealNumber_WithDecimal_ReturnsTrue()
        => Assert.IsTrue(typeof(decimal).IsRealNumber());

    [TestMethod]
    public void IsRealNumber_WithHalf_ReturnsTrue()
        => Assert.IsTrue(typeof(Half).IsRealNumber());

    [TestMethod]
    public void IsRealNumber_WithInt_ReturnsFalse()
        => Assert.IsFalse(typeof(int).IsRealNumber());

    [TestMethod]
    public void IsRealNumber_WithString_ReturnsFalse()
        => Assert.IsFalse(typeof(string).IsRealNumber());

    #endregion

    #region IsNumber Tests

    [TestMethod]
    public void IsNumber_WithWholeNumber_ReturnsTrue()
        => Assert.IsTrue(typeof(int).IsNumber());

    [TestMethod]
    public void IsNumber_WithRealNumber_ReturnsTrue()
        => Assert.IsTrue(typeof(double).IsNumber());

    [TestMethod]
    public void IsNumber_WithInt128_ReturnsTrue()
        => Assert.IsTrue(typeof(Int128).IsNumber());

    [TestMethod]
    public void IsNumber_WithHalf_ReturnsTrue()
        => Assert.IsTrue(typeof(Half).IsNumber());

    [TestMethod]
    public void IsNumber_WithString_ReturnsFalse()
        => Assert.IsFalse(typeof(string).IsNumber());

    [TestMethod]
    public void IsNumber_WithBool_ReturnsFalse()
        => Assert.IsFalse(typeof(bool).IsNumber());

    #endregion

    #region NumericMaxMin Tests - .NET 9 Support

    [TestMethod]
    public void NumericMaxMin_WithInt_ReturnsCorrectBounds()
    {
        TypeHelpers.NumericMaxMin(typeof(int), out var max, out var min);
        Assert.AreEqual(int.MaxValue, max);
        Assert.AreEqual(int.MinValue, min);
    }

    [TestMethod]
    public void NumericMaxMin_WithByte_ReturnsCorrectBounds()
    {
        TypeHelpers.NumericMaxMin(typeof(byte), out var max, out var min);
        Assert.AreEqual(byte.MaxValue, max);
        Assert.AreEqual(byte.MinValue, min);
    }

    [TestMethod]
    public void NumericMaxMin_WithDouble_ReturnsCorrectBounds()
    {
        TypeHelpers.NumericMaxMin(typeof(double), out var max, out var min);
        Assert.AreEqual(double.MaxValue, max);
        Assert.AreEqual(double.MinValue, min);
    }

    [TestMethod]
    public void NumericMaxMin_WithHalf_ReturnsCorrectBounds()
    {
        TypeHelpers.NumericMaxMin(typeof(Half), out var max, out var min);
        Assert.AreEqual((double)Half.MaxValue, max);
        Assert.AreEqual((double)Half.MinValue, min);
    }

    [TestMethod]
    public void NumericMaxMin_WithNInt_ReturnsCorrectBounds()
    {
        TypeHelpers.NumericMaxMin(typeof(nint), out var max, out var min);
        Assert.AreEqual(nint.MaxValue, max);
        Assert.AreEqual(nint.MinValue, min);
    }

    [TestMethod]
    public void NumericMaxMin_WithNUInt_ReturnsCorrectBounds()
    {
        TypeHelpers.NumericMaxMin(typeof(nuint), out var max, out var min);
        Assert.AreEqual(nuint.MaxValue, max);
        Assert.AreEqual(nuint.MinValue, min);
    }

    [TestMethod]
    public void NumericMaxMin_WithInt128_ReturnsApproximateBounds()
    {
        TypeHelpers.NumericMaxMin(typeof(Int128), out var max, out var min);
        Assert.AreEqual(double.MaxValue, max);
        Assert.AreEqual(double.MinValue, min);
    }

    [TestMethod]
    public void NumericMaxMin_WithUInt128_ReturnsApproximateBounds()
    {
        TypeHelpers.NumericMaxMin(typeof(UInt128), out var max, out var min);
        Assert.AreEqual(double.MaxValue, max);
        Assert.AreEqual(0.0, min);
    }

    [TestMethod]
    public void NumericMaxMin_WithNonNumericType_ThrowsException()
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            TypeHelpers.NumericMaxMin(typeof(string), out _, out _));

    #endregion

    #region GetUnderlyingType Tests

    [TestMethod]
    public void GetUnderlyingType_WithProperty_ReturnsPropertyType()
    {
        var prop = typeof(TestClass).GetProperty(nameof(TestClass.Name));
        var result = prop.GetUnderlyingType();
        Assert.AreEqual(typeof(string), result);
    }

    [TestMethod]
    public void GetUnderlyingType_WithField_ReturnsFieldType()
    {
        var field = typeof(TestClass).GetField(nameof(TestClass.PublicField));
        var result = field.GetUnderlyingType();
        Assert.AreEqual(typeof(int), result);
    }

    [TestMethod]
    public void GetUnderlyingType_WithNull_ThrowsException()
        => Assert.Throws<ArgumentNullException>(() =>
            TypeHelpers.GetUnderlyingType(null));

    #endregion

    #region GetValue and SetValue Tests

    [TestMethod]
    public void GetValue_WithProperty_ReturnsValue()
    {
        var obj = new TestClass { Name = "Test" };
        var prop = typeof(TestClass).GetProperty(nameof(TestClass.Name));
        var result = prop.GetValue(obj);
        Assert.AreEqual("Test", result);
    }

    [TestMethod]
    public void SetValue_WithProperty_SetsValue()
    {
        var obj = new TestClass();
        var prop = typeof(TestClass).GetProperty(nameof(TestClass.Name));
        prop.SetValue(obj, "NewValue");
        Assert.AreEqual("NewValue", obj.Name);
    }

    [TestMethod]
    public void GetValue_WithField_ReturnsValue()
    {
        var obj = new TestClass { PublicField = 42 };
        var field = typeof(TestClass).GetField(nameof(TestClass.PublicField));
        var result = field.GetValue(obj);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void SetValue_WithField_SetsValue()
    {
        var obj = new TestClass();
        var field = typeof(TestClass).GetField(nameof(TestClass.PublicField));
        field.SetValue(obj, 99);
        Assert.AreEqual(99, obj.PublicField);
    }

    #endregion

    #region Additional ConvertValue Tests

    [TestMethod]
    public void ConvertValue_StringToTimeSpan_ConvertsCorrectly()
    {
        var result = TypeHelpers.ConvertValue(typeof(TimeSpan), "01:30:00");
        Assert.AreEqual(new TimeSpan(1, 30, 0), result);
    }

    [TestMethod]
    public void ConvertValue_ObjectToSameType_ReturnsSameValue()
    {
        var obj = new object();
        var result = TypeHelpers.ConvertValue(typeof(object), obj);
        Assert.AreSame(obj, result);
    }

    [TestMethod]
    public void ConvertValue_NullToNullableType_ReturnsNull()
    {
        var result = TypeHelpers.ConvertValue(typeof(int?), (object)null);
        Assert.IsNull(result);
    }

    #endregion

    #region CanWrite Tests

    [TestMethod]
    public void CanWrite_WithWritableProperty_ReturnsTrue()
    {
        var prop = typeof(TestClass).GetProperty(nameof(TestClass.Name));
        Assert.IsTrue(prop.CanWrite());
    }

    [TestMethod]
    public void CanWrite_WithReadOnlyProperty_ReturnsFalse()
    {
        var prop = typeof(TestClass).GetProperty(nameof(TestClass.ReadOnlyProperty));
        Assert.IsFalse(prop.CanWrite());
    }

    [TestMethod]
    public void CanWrite_WithField_ReturnsTrue()
    {
        var field = typeof(TestClass).GetField(nameof(TestClass.PublicField));
        Assert.IsTrue(field.CanWrite());
    }

    [TestMethod]
    public void CanWrite_WithReadOnlyField_ReturnsFalse()
    {
        var field = typeof(TestClass).GetField(nameof(TestClass.ReadOnlyField));
        Assert.IsFalse(field.CanWrite());
    }

    #endregion

    #region GetConstructorNoParameters Tests

    [TestMethod]
    public void GetConstructorNoParameters_WithDefaultConstructor_ReturnsConstructor()
    {
        var ctor = typeof(List<int>).GetConstructorNoParameters();
        Assert.IsNotNull(ctor);
        Assert.HasCount(0, ctor.GetParameters());
    }

    [TestMethod]
    public void GetConstructorNoParameters_WithoutDefaultConstructor_ReturnsNull()
    {
        var ctor = typeof(TestClassNoDefaultCtor).GetConstructorNoParameters();
        Assert.IsNull(ctor);
    }

    #endregion

    #region GetPropertiesPublicInstanceRead Tests

    [TestMethod]
    public void GetPropertiesPublicInstanceRead_ReturnsReadableProperties()
    {
        var props = typeof(TestClass).GetPropertiesPublicInstanceRead();
        Assert.IsTrue(props.Length > 0);
        Assert.IsTrue(props.All(p => p.CanRead));
    }

    #endregion

    #region GetPropertiesPublicInstanceReadWrite Tests

    [TestMethod]
    public void GetPropertiesPublicInstanceReadWrite_ReturnsReadWriteProperties()
    {
        var props = typeof(TestClassWithReadWrite).GetPropertiesPublicInstanceReadWrite();
        Assert.IsTrue(props.All(p => p.CanRead && p.CanWrite));
    }

    #endregion

    #region IsA Tests

    [TestMethod]
    public void IsA_Generic_WithDerivedType_ReturnsTrue()
        => Assert.IsTrue(typeof(List<int>).IsA<IList>());

    [TestMethod]
    public void IsA_Generic_WithUnrelatedType_ReturnsFalse()
        => Assert.IsFalse(typeof(string).IsA<IList>());

    [TestMethod]
    public void IsA_NonGeneric_WithDerivedType_ReturnsTrue()
        => Assert.IsTrue(typeof(List<int>).IsA(typeof(IEnumerable)));

    [TestMethod]
    public void IsA_NonGeneric_WithUnrelatedType_ReturnsFalse()
        => Assert.IsFalse(typeof(string).IsA(typeof(IList)));

    #endregion

    #region MemberWalk Tests

    [TestMethod]
    public void MemberWalk_VisitsAllMembers()
    {
        var visitedMembers = new List<string>();

        typeof(TestClass).MemberWalk(
            BindingFlags.Public | BindingFlags.Instance,
            (context, type, member) => context,
            (context, type, member) => visitedMembers.Add(member?.Name ?? "root"),
            null,
            default(object));

        Assert.IsTrue(visitedMembers.Count > 0);
    }

    [TestMethod]
    public void MemberWalk_PreventsCycles()
    {
        var visitCount = 0;

        typeof(TestClass).MemberWalk(
            BindingFlags.Public | BindingFlags.Instance,
            (context, type, member) => context,
            (context, type, member) => visitCount++,
            null,
            default(object));

        Assert.IsTrue(visitCount > 0);
    }

    #endregion

    #region Helper Classes

    public class TestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string ReadOnlyProperty => "ReadOnly";
        public int PublicField;
        public readonly int ReadOnlyField = 42;
    }

    public class TestClassWithReadWrite
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public double Score { get; set; }
    }

    public class TestClassNoDefaultCtor
    {
        public TestClassNoDefaultCtor(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    #endregion
}
