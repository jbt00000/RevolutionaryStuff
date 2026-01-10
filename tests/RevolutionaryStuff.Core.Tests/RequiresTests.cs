using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class RequiresTests
{
    #region Existing Tests

    [TestMethod]
    public void SingleCallOnce()
    {
        var called = false;
        Requires.SingleCall(ref called);
    }

    [TestMethod]
    public void SingleCallMultiple()
    {
        var called = false;
        Requires.SingleCall(ref called);
        Assert.Throws<Exception>(() => Requires.SingleCall(ref called));
    }

    [TestMethod]
    public void TextCorrect()
        => Requires.Text("jason", "z");

    [TestMethod]
    public void TextTooShort()
        => Assert.ThrowsExactly<ArgumentException>(() => Requires.Text("tooshort", "z", minLen: 20));

    [TestMethod]
    public void TextTooLong()
        => Assert.ThrowsExactly<ArgumentException>(() => Requires.Text("toolong", "z", maxLen: 3));

    [TestMethod]
    public void TextEmpty()
        => Assert.Throws<Exception>(() => Requires.Text("", "z"));

    [TestMethod]
    public void TextNull()
        => Assert.Throws<Exception>(() => Requires.Text(null, "z"));

    [TestMethod]
    public void NullValid()
        => Requires.Null(null, "hasnodata");

    [TestMethod]
    public void NullInvalid()
        => Assert.Throws<ArgumentException>(() => Requires.Null("I'm supposed to be null", "hasdata"));

    [TestMethod]
    public void NonNullInvalidCheckingCallerArgumentExpression()
    {
        string myVariableName = null;
        try
        {
            ArgumentNullException.ThrowIfNull(myVariableName);
            Assert.Fail($"Expected {nameof(ArgumentNullException)}");
        }
        catch (ArgumentNullException anex)
        {
            Assert.AreEqual(nameof(myVariableName), anex.ParamName);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected {nameof(ArgumentNullException)} instead of {ex.GetType().Name}");
        }
    }

    [TestMethod]
    public void XmlWithNonXmlData()
        => Assert.Throws<Exception>(() => Requires.Xml("honey, I don't think this is xml!", "nonxml"));

    [TestMethod]
    public void XmlWithBoringXmlData()
        => Requires.Xml("<root>yeah, we found xml!</root>", "xml");

    #endregion

    #region URL Tests

    [TestMethod]
    public void Url_Valid_Succeeds()
    {
        Requires.Url("https://www.example.com");
    }

    [TestMethod]
    public void Url_Invalid_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Requires.Url("not a url"));
    }

    [TestMethod]
    public void Url_Null_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Requires.Url(null));
    }

    #endregion

    #region SetMembership Tests

    [TestMethod]
    public void SetMembership_ValueInSet_Succeeds()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        Requires.SetMembership(set, "numbers", 2, "value");
    }

    [TestMethod]
    public void SetMembership_ValueNotInSet_ThrowsException()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.SetMembership(set, "numbers", 5, "value"));
    }

    [TestMethod]
    public void SetMembership_NullInputAllowed_Succeeds()
    {
        var set = new HashSet<string> { "a", "b" };
        Requires.SetMembership(set, "strings", null, "value", nullInputOk: true);
    }

    #endregion

    #region ArrayArg Tests

    [TestMethod]
    public void ArrayArg_ValidArgs_Succeeds()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        Requires.ArrayArg(array, 1, 3, "array");
    }

    [TestMethod]
    public void ArrayArg_NegativeOffset_ThrowsException()
    {
        var array = new[] { 1, 2, 3 };
        Assert.Throws<ArgumentException>(() => Requires.ArrayArg(array, -1, 2, "array"));
    }

    [TestMethod]
    public void ArrayArg_SizeTooLarge_ThrowsException()
    {
        var array = new[] { 1, 2, 3 };
        Assert.Throws<ArgumentException>(() => Requires.ArrayArg(array, 1, 10, "array"));
    }

    #endregion

    #region ListArg Tests

    [TestMethod]
    public void ListArg_ValidList_Succeeds()
    {
        var list = new List<int> { 1, 2, 3 };
        Requires.ListArg(list, "list");
    }

    [TestMethod]
    public void ListArg_TooSmall_ThrowsException()
    {
        var list = new List<int> { 1, 2 };
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.ListArg(list, "list", minSize: 5));
    }

    [TestMethod]
    public void ListArg_Null_ThrowsException()
    {
        List<int> list = null;
        Assert.Throws<ArgumentNullException>(() => Requires.ListArg(list, "list"));
    }

    #endregion

    #region Valid Tests

    [TestMethod]
    public void Valid_ValidObject_Succeeds()
    {
        var obj = new ValidObject(true);
        Requires.Valid(obj);
    }

    [TestMethod]
    public void Valid_InvalidObject_ThrowsException()
    {
        var obj = new ValidObject(false);
        Assert.Throws<InvalidOperationException>(() => Requires.Valid(obj));
    }

    [TestMethod]
    public void Valid_NullAllowed_Succeeds()
    {
        ValidObject obj = null;
        Requires.Valid(obj, canBeNull: true);
    }

    private class ValidObject : IValidate
    {
        private readonly bool _isValid;
        public ValidObject(bool isValid) => _isValid = isValid;
        public void Validate()
        {
            if (!_isValid) throw new InvalidOperationException("Not valid");
        }
    }

    #endregion

    #region True/False Tests

    [TestMethod]
    public void True_WithTrueValue_Succeeds()
    {
        Requires.True(true);
    }

    [TestMethod]
    public void True_WithFalseValue_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.True(false));
    }

    [TestMethod]
    public void False_WithFalseValue_Succeeds()
    {
        Requires.False(false);
    }

    [TestMethod]
    public void False_WithTrueValue_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.False(true));
    }

    #endregion

    #region FileExtension Tests

    [TestMethod]
    public void FileExtension_Valid_Succeeds()
    {
        Requires.FileExtension(".txt");
    }

    [TestMethod]
    public void FileExtension_Invalid_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Requires.FileExtension("txt"));
    }

    [TestMethod]
    public void FileExtension_WithFilename_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Requires.FileExtension("file.txt"));
    }

    #endregion

    #region HasData/HasNoData Tests

    [TestMethod]
    public void HasData_WithData_Succeeds()
    {
        var list = new[] { 1, 2, 3 };
        Requires.HasData(list);
    }

    [TestMethod]
    public void HasData_Empty_ThrowsException()
    {
        var list = Array.Empty<int>();
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.HasData(list));
    }

    [TestMethod]
    public void HasNoData_Empty_Succeeds()
    {
        var list = Array.Empty<int>();
        Requires.HasNoData(list);
    }

    [TestMethod]
    public void HasNoData_WithData_ThrowsException()
    {
        var list = new[] { 1 };
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.HasNoData(list));
    }

    [TestMethod]
    public void HasNoData_Null_Succeeds()
    {
        Requires.HasNoData(null);
    }

    #endregion

    #region AreEqual Tests

    [TestMethod]
    public void AreEqual_EqualValues_Succeeds()
    {
        Requires.AreEqual(5, 5);
    }

    [TestMethod]
    public void AreEqual_DifferentValues_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.AreEqual(5, 10));
    }

    [TestMethod]
    public void AreEqual_Strings_Succeeds()
    {
        Requires.AreEqual("hello", "hello");
    }

    #endregion

    #region ExactlyOneNonNull Tests

    [TestMethod]
    public void ExactlyOneNonNull_OneNonNull_Succeeds()
    {
        Requires.ExactlyOneNonNull(null, "value", null);
    }

    [TestMethod]
    public void ExactlyOneNonNull_AllNull_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Requires.ExactlyOneNonNull(null, null, null));
    }

    [TestMethod]
    public void ExactlyOneNonNull_TwoNonNull_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Requires.ExactlyOneNonNull("a", "b", null));
    }

    #endregion

    #region IsType Tests

    [TestMethod]
    public void IsType_ValidAssignment_Succeeds()
    {
        Requires.IsType(typeof(string), typeof(object));
    }

    [TestMethod]
    public void IsType_InvalidAssignment_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Requires.IsType(typeof(int), typeof(string)));
    }

    #endregion

    #region Numeric Range Tests

    [TestMethod]
    public void Zero_WithZero_Succeeds()
    {
        Requires.Zero(0.0);
    }

    [TestMethod]
    public void Zero_WithNonZero_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.Zero(1.0));
    }

    [TestMethod]
    public void NonNegative_Double_WithZero_Succeeds()
    {
        Requires.NonNegative(0.0);
    }

    [TestMethod]
    public void NonNegative_Double_WithPositive_Succeeds()
    {
        Requires.NonNegative(5.0);
    }

    [TestMethod]
    public void NonNegative_Double_WithNegative_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.NonNegative(-1.0));
    }

    [TestMethod]
    public void NonNegative_Long_WithPositive_Succeeds()
    {
        Requires.NonNegative(10L);
    }

    [TestMethod]
    public void Positive_Double_WithPositive_Succeeds()
    {
        Requires.Positive(1.0);
    }

    [TestMethod]
    public void Positive_Double_WithZero_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.Positive(0.0));
    }

    [TestMethod]
    public void Positive_Long_WithNegative_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.Positive(-5L));
    }

    [TestMethod]
    public void Negative_Double_WithNegative_Succeeds()
    {
        Requires.Negative(-1.0);
    }

    [TestMethod]
    public void Negative_Double_WithZero_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.Negative(0.0));
    }

    [TestMethod]
    public void NonPositive_Double_WithZero_Succeeds()
    {
        Requires.NonPositive(0.0);
    }

    [TestMethod]
    public void NonPositive_Double_WithPositive_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.NonPositive(1.0));
    }

    #endregion

    #region Match Tests

    [TestMethod]
    public void Match_Matches_Succeeds()
    {
        var regex = new Regex(@"^\d{3}-\d{4}$");
        Requires.Match(regex, "123-4567");
    }

    [TestMethod]
    public void Match_DoesNotMatch_ThrowsException()
    {
        var regex = new Regex(@"^\d{3}-\d{4}$");
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.Match(regex, "invalid"));
    }

    #endregion

    #region Text Additional Tests

    [TestMethod]
    public void Text_AllowNull_Succeeds()
    {
        Requires.Text(null, "arg", allowNull: true, minLen: 0);
    }

    [TestMethod]
    public void Text_MinMaxValid_Succeeds()
    {
        Requires.Text("hello", "arg", minLen: 3, maxLen: 10);
    }

    [TestMethod]
    public void Text_MinGreaterThanMax_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Requires.Text("hello", "arg", minLen: 10, maxLen: 5));
    }

    #endregion

    #region EmailAddress Tests

    [TestMethod]
    public void EmailAddress_Valid_Succeeds()
    {
        Requires.EmailAddress("test@example.com");
    }

    [TestMethod]
    public void EmailAddress_Invalid_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.EmailAddress("notanemail"));
    }

    #endregion

    #region Stream Tests

    [TestMethod]
    public void ReadableStreamArg_ReadableStream_Succeeds()
    {
        using var stream = new MemoryStream();
        Requires.ReadableStreamArg(stream);
    }

    [TestMethod]
    public void WriteableStreamArg_WriteableStream_Succeeds()
    {
        using var stream = new MemoryStream();
        Requires.WriteableStreamArg(stream);
    }

    [TestMethod]
    public void StreamArg_Null_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Requires.ReadableStreamArg(null));
    }

    #endregion

    #region Between Tests

    [TestMethod]
    public void Between_InRange_Succeeds()
    {
        Requires.Between(5, "val", minLength: 1, maxLength: 10);
    }

    [TestMethod]
    public void Between_BelowMin_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.Between(0, "val", minLength: 1, maxLength: 10));
    }

    [TestMethod]
    public void Between_AboveMax_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.Between(15, "val", minLength: 1, maxLength: 10));
    }

    [TestMethod]
    public void Between_NoLimits_Succeeds()
    {
        Requires.Between(long.MinValue);
        Requires.Between(long.MaxValue);
    }

    #endregion

    #region Buffer Tests

    [TestMethod]
    public void Buffer_ValidSize_Succeeds()
    {
        var buffer = new byte[10];
        Requires.Buffer(buffer, "buf", minLength: 5, maxLength: 15);
    }

    [TestMethod]
    public void Buffer_Null_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Requires.Buffer(null));
    }

    [TestMethod]
    public void Buffer_TooSmall_ThrowsException()
    {
        var buffer = new byte[2];
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.Buffer(buffer, "buf", minLength: 5));
    }

    #endregion

    #region PortNumber Tests

    [TestMethod]
    public void PortNumber_Valid_Succeeds()
    {
        Requires.PortNumber(80);
        Requires.PortNumber(443);
        Requires.PortNumber(8080);
    }

    [TestMethod]
    public void PortNumber_Zero_AllowZero_Succeeds()
    {
        Requires.PortNumber(0, allowZero: true);
    }

    [TestMethod]
    public void PortNumber_Zero_DisallowZero_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.PortNumber(0, allowZero: false));
    }

    [TestMethod]
    public void PortNumber_TooLarge_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.PortNumber(70000));
    }

    [TestMethod]
    public void PortNumber_Negative_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Requires.PortNumber(-1));
    }

    #endregion

    #region DataTable Tests

    [TestMethod]
    public void ZeroRows_EmptyTable_Succeeds()
    {
        var dt = new DataTable();
        Requires.ZeroRows(dt);
    }

    [TestMethod]
    public void ZeroRows_WithRows_ThrowsException()
    {
        var dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Rows.Add("value");
        Assert.Throws<ArgumentException>(() => Requires.ZeroRows(dt));
    }

    [TestMethod]
    public void ZeroColumns_EmptyTable_Succeeds()
    {
        var dt = new DataTable();
        Requires.ZeroColumns(dt);
    }

    [TestMethod]
    public void ZeroColumns_WithColumns_ThrowsException()
    {
        var dt = new DataTable();
        dt.Columns.Add("Col1");
        Assert.Throws<ArgumentException>(() => Requires.ZeroColumns(dt));
    }

    #endregion

    #region XML Additional Tests

    [TestMethod]
    public void Xml_WithAttributes_Succeeds()
    {
        Requires.Xml("<root attr='value'>content</root>");
    }

    [TestMethod]
    public void Xml_Empty_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Requires.Xml(""));
    }

    #endregion

    #region CallerArgumentExpression Tests

    [TestMethod]
    public void CallerArgumentExpression_CapturesVariableName()
    {
        int myNumber = -1;
        try
        {
            Requires.Positive(myNumber);
            Assert.Fail("Expected exception");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Assert.AreEqual(nameof(myNumber), ex.ParamName);
        }
    }

    #endregion
}
