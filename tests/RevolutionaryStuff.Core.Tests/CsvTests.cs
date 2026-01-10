using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class CsvTests
{
    #region Existing Format Tests

    [TestMethod]
    public void FormatCellNoEscapeSingleChar()
    {
        Assert.AreEqual("a", CSV.Format("a"));
    }

    [TestMethod]
    public void FormatCellNoEscapeMultipleChars()
    {
        Assert.AreEqual("aa", CSV.Format("aa"));
    }

    [TestMethod]
    public void FormatCellNoEscapeMultiplePreSpace()
    {
        Assert.AreEqual(" a", CSV.Format(" a"));
    }

    [TestMethod]
    public void FormatCellNoEscapeMultiplePostSpace()
    {
        Assert.AreEqual("a ", CSV.Format("a "));
    }

    [TestMethod]
    public void FormatCellNoEscapeMultiplePreTab()
    {
        Assert.AreEqual("a\t", CSV.Format("a\t"));
    }

    [TestMethod]
    public void FormatCellNoEscapeMultiplePostTab()
    {
        Assert.AreEqual("\ta", CSV.Format("\ta"));
    }

    [TestMethod]
    public void FormatCellWithNewlineR()
    {
        Assert.AreEqual("\"a\rb\"", CSV.Format("a\rb"));
    }

    [TestMethod]
    public void FormatCellWithNewlineN()
    {
        Assert.AreEqual("\"a\nb\"", CSV.Format("a\nb"));
    }

    [TestMethod]
    public void FormatCellWithEmbeddedCommaWithCommaDelim()
    {
        Assert.AreEqual("\"a,b\"", CSV.Format("a,b"));
    }

    [TestMethod]
    public void FormatEvilLineDelimComma()
    {
        Assert.AreEqual(
            @"a,"","", b,c ,d d,""e,e"",""f" + "\n" + @"f"",g|g",
            CSV.FormatLine(new[] { "a", ",", " b", "c ", "d d", "e,e", "f\nf", "g|g" },
            false));
    }

    [TestMethod]
    public void FormatEvilLineDelimPipe()
    {
        var sb = new StringBuilder();
        CSV.FormatLine(sb, new[] { "a", ",", " b", "c ", "d d", "e,e", "f\nf", "g|g" }, false, '|');
        var actual = sb.ToString();
        Assert.AreEqual(@"a|,| b|c |d d|e,e|""f" + "\n" + @"f""|""g|g""", actual);
    }

    #endregion

    #region Additional Format Tests

    [TestMethod]
    public void Format_WithQuote_EscapedAndQuoted()
    {
        var result = CSV.Format("hello\"world");
        Assert.AreEqual("\"hello\"\"world\"", result);
    }

    [TestMethod]
    public void Format_Null_ReturnsNull()
    {
        var result = CSV.Format(null);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Format_Empty_ReturnsEmpty()
    {
        var result = CSV.Format("");
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void Format_WithPipeDelimiter_QuotesOnPipe()
    {
        var triggers = new[] { '\r', '\n', '\"', '|' };
        var result = CSV.Format("hello|world", triggers);
        Assert.AreEqual("\"hello|world\"", result);
    }

    #endregion

    #region ToCsv Tests

    [TestMethod]
    public void ToCsv_SimpleValues()
    {
        var values = new[] { "a", "b", "c" };
        var result = values.ToCsv();
        Assert.AreEqual("a,b,c", result);
    }

    [TestMethod]
    public void ToCsv_WithEOL()
    {
        var values = new[] { "a", "b" };
        var result = values.ToCsv(eol: true);
        Assert.AreEqual("a,b\r\n", result);
    }

    [TestMethod]
    public void ToCsv_WithQuotedValues()
    {
        var values = new[] { "a", "b,c", "d" };
        var result = values.ToCsv();
        Assert.AreEqual("a,\"b,c\",d", result);
    }

    #endregion

    #region FormatLine Additional Tests

    [TestMethod]
    public void FormatLine_WithNullValue()
    {
        var objects = new object[] { "a", null, "c" };
        var result = CSV.FormatLine(objects, eol: false);
        Assert.AreEqual("a,,c", result);
    }

    [TestMethod]
    public void FormatLine_WithEOL()
    {
        var objects = new object[] { "a", "b" };
        var result = CSV.FormatLine(objects, eol: true);
        Assert.AreEqual("a,b\r\n", result);
    }

    [TestMethod]
    public void FormatLine_DictionaryEntry()
    {
        var entry = new System.Collections.DictionaryEntry("key", "value");
        var result = CSV.FormatLine(entry);
        Assert.AreEqual("key,value\r\n", result);
    }

    #endregion

    #region ParseLine Tests

    [TestMethod]
    public void ParseLine_Simple()
    {
        var result = CSV.ParseLine("a,b,c");
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result);
    }

    [TestMethod]
    public void ParseLine_WithQuotedField()
    {
        var result = CSV.ParseLine("a,\"b,c\",d");
        CollectionAssert.AreEqual(new[] { "a", "b,c", "d" }, result);
    }

    [TestMethod]
    public void ParseLine_WithEscapedQuote()
    {
        var result = CSV.ParseLine("a,\"b\"\"c\",d");
        CollectionAssert.AreEqual(new[] { "a", "b\"c", "d" }, result);
    }

    [TestMethod]
    public void ParseLine_WithNewlineInQuotes()
    {
        var result = CSV.ParseLine("a,\"b\nc\",d");
        CollectionAssert.AreEqual(new[] { "a", "b\nc", "d" }, result);
    }

    [TestMethod]
    public void ParseLine_Empty()
    {
        var result = CSV.ParseLine("");
        Assert.HasCount(0, result);
    }

    [TestMethod]
    public void ParseLine_Null()
    {
        var result = CSV.ParseLine(null);
        Assert.HasCount(0, result);
    }

    [TestMethod]
    public void ParseLine_EmptyFields()
    {
        var result = CSV.ParseLine("a,,c");
        CollectionAssert.AreEqual(new[] { "a", "", "c" }, result);
    }

    [TestMethod]
    public void ParseLine_PipeDelimiter()
    {
        var result = CSV.ParseLine("a|b|c", CSV.FieldDelimPipe);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result);
    }

    [TestMethod]
    public void ParseLine_TrailingComma()
    {
        var result = CSV.ParseLine("a,b,");
        CollectionAssert.AreEqual(new[] { "a", "b", "" }, result);
    }

    [TestMethod]
    public void ParseLine_LeadingComma()
    {
        var result = CSV.ParseLine(",a,b");
        CollectionAssert.AreEqual(new[] { "", "a", "b" }, result);
    }

    #endregion

    #region ParseText Tests

    [TestMethod]
    public void ParseText_MultipleLines()
    {
        var csv = "a,b,c\r\nd,e,f\r\ng,h,i";
        var result = CSV.ParseText(csv);
        Assert.HasCount(3, result);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result[0]);
        CollectionAssert.AreEqual(new[] { "d", "e", "f" }, result[1]);
        CollectionAssert.AreEqual(new[] { "g", "h", "i" }, result[2]);
    }

    [TestMethod]
    public void ParseText_WithQuotedFields()
    {
        var csv = "\"a,1\",\"b,2\"\r\n\"c,3\",\"d,4\"";
        var result = CSV.ParseText(csv);
        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(new[] { "a,1", "b,2" }, result[0]);
        CollectionAssert.AreEqual(new[] { "c,3", "d,4" }, result[1]);
    }

    [TestMethod]
    public void ParseText_WithMultilineValues()
    {
        var csv = "a,\"b\nc\",d\r\ne,f,g";
        var result = CSV.ParseText(csv);
        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(new[] { "a", "b\nc", "d" }, result[0]);
        CollectionAssert.AreEqual(new[] { "e", "f", "g" }, result[1]);
    }

    [TestMethod]
    public void ParseText_Empty()
    {
        var result = CSV.ParseText("");
        Assert.HasCount(0, result);
    }

    [TestMethod]
    public void ParseText_SingleLine()
    {
        var result = CSV.ParseText("a,b,c");
        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result[0]);
    }

    [TestMethod]
    public void ParseText_PipeDelimiter()
    {
        var csv = "a|b|c\r\nd|e|f";
        var result = CSV.ParseText(csv, CSV.FieldDelimPipe);
        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result[0]);
    }

    #endregion

    #region ParseTextEnumerable Tests

    [TestMethod]
    public void ParseTextEnumerable_LazyEvaluation()
    {
        var csv = "a,b,c\r\nd,e,f\r\ng,h,i";
        var enumerable = CSV.ParseTextEnumerable(csv);

        var count = 0;
        foreach (var row in enumerable)
        {
            count++;
            Assert.IsNotNull(row);
        }
        Assert.AreEqual(3, count);
    }

    [TestMethod]
    public void ParseTextEnumerable_CanEnumerateMultipleTimes()
    {
        var csv = "a,b\r\nc,d";
        var enumerable = CSV.ParseTextEnumerable(csv);

        var firstPass = enumerable.ToList();
        var secondPass = enumerable.ToList();

        Assert.HasCount(2, firstPass);
        Assert.HasCount(2, secondPass);
    }

    #endregion

    #region ParseIntegerRow Tests

    [TestMethod]
    public void ParseIntegerRow_ValidIntegers()
    {
        var result = CSV.ParseIntegerRow("1,2,3,4,5");
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [TestMethod]
    public void ParseIntegerRow_NegativeNumbers()
    {
        var result = CSV.ParseIntegerRow("-1,0,1");
        CollectionAssert.AreEqual(new[] { -1, 0, 1 }, result);
    }

    [TestMethod]
    public void ParseIntegerRow_Empty()
    {
        var result = CSV.ParseIntegerRow("");
        Assert.HasCount(0, result);
    }

    [TestMethod]
    public void ParseIntegerRow_Null()
    {
        var result = CSV.ParseIntegerRow(null);
        Assert.HasCount(0, result);
    }

    [TestMethod]
    public void ParseIntegerRow_InvalidInteger_ThrowsException()
    {
        Assert.Throws<FormatException>(() => CSV.ParseIntegerRow("1,abc,3"));
    }

    #endregion

    #region ParseRow<T> Tests

    [TestMethod]
    public void ParseRow_WithConverter()
    {
        var result = CSV.ParseRow("1.5,2.5,3.5", s => double.Parse(s));
        CollectionAssert.AreEqual(new[] { 1.5, 2.5, 3.5 }, result);
    }

    [TestMethod]
    public void ParseRow_StringToInt()
    {
        var result = CSV.ParseRow("10,20,30", int.Parse);
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, result);
    }

    [TestMethod]
    public void ParseRow_CustomConverter()
    {
        var result = CSV.ParseRow("a,b,c", s => s.ToUpper());
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, result);
    }

    [TestMethod]
    public void ParseRow_Empty()
    {
        var result = CSV.ParseRow<int>("", int.Parse);
        Assert.HasCount(0, result);
    }

    #endregion

    #region StreamReader Tests

    [TestMethod]
    public void ParseText_FromStreamReader()
    {
        var csv = "a,b,c\r\nd,e,f";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        using var reader = new StreamReader(stream);

        var result = CSV.ParseText(reader);
        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result[0]);
        CollectionAssert.AreEqual(new[] { "d", "e", "f" }, result[1]);
    }

    #endregion

    #region Round-Trip Tests

    [TestMethod]
    public void RoundTrip_SimpleData()
    {
        var original = new[] { new[] { "a", "b", "c" }, new[] { "d", "e", "f" } };

        // Format
        var lines = original.Select(row => CSV.FormatLine(row, eol: true)).ToArray();
        var csv = string.Join("", lines);

        // Parse
        var parsed = CSV.ParseText(csv);

        Assert.HasCount(original.Length, parsed);
        for (var i = 0; i < original.Length; i++)
        {
            CollectionAssert.AreEqual(original[i], parsed[i]);
        }
    }

    [TestMethod]
    public void RoundTrip_ComplexData()
    {
        var original = new[] {
            new[] { "simple", "has,comma", "has\"quote", "has\nnewline" },
            new[] { "123", "", "empty value", "end" }
        };

        // Format
        var lines = original.Select(row => CSV.FormatLine(row, eol: true)).ToArray();
        var csv = string.Join("", lines);

        // Parse
        var parsed = CSV.ParseText(csv);

        Assert.HasCount(original.Length, parsed);
        for (var i = 0; i < original.Length; i++)
        {
            CollectionAssert.AreEqual(original[i], parsed[i]);
        }
    }

    [TestMethod]
    public void RoundTrip_PipeDelimiter()
    {
        var original = new[] { "a", "b|c", "d" };

        // Format with pipe delimiter
        var sb = new StringBuilder();
        CSV.FormatLine(sb, original, eol: true, fieldDelim: CSV.FieldDelimPipe);
        var csv = sb.ToString();

        // Parse with pipe delimiter
        var parsed = CSV.ParseLine(csv.TrimEnd('\r', '\n'), CSV.FieldDelimPipe);

        CollectionAssert.AreEqual(original, parsed);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCase_OnlyQuotes()
    {
        var result = CSV.ParseLine("\"\"\"\"");
        CollectionAssert.AreEqual(new[] { "\"" }, result);
    }

    [TestMethod]
    public void EdgeCase_EmptyQuotedField()
    {
        var result = CSV.ParseLine("a,\"\",c");
        CollectionAssert.AreEqual(new[] { "a", "", "c" }, result);
    }

    [TestMethod]
    public void EdgeCase_ConsecutiveCommas()
    {
        var result = CSV.ParseLine("a,,,d");
        CollectionAssert.AreEqual(new[] { "a", "", "", "d" }, result);
    }

    [TestMethod]
    public void EdgeCase_CRLFVariations()
    {
        var csv = "a,b\rc,d\ne,f\r\ng,h";
        var result = CSV.ParseText(csv);
        Assert.HasCount(4, result);
    }

    #endregion

    #region Performance Tests

    [TestMethod]
    public void LargeData_ManyRows()
    {
        var rowCount = 1000;
        var rows = new List<string>();
        for (var i = 0; i < rowCount; i++)
        {
            rows.Add($"{i},value{i},data{i}");
        }
        var csv = string.Join("\r\n", rows);

        var result = CSV.ParseText(csv);
        Assert.AreEqual(rowCount, result.Length);
    }

    [TestMethod]
    public void LargeData_ManyColumns()
    {
        var columnCount = 100;
        var values = Enumerable.Range(1, columnCount).Select(i => $"col{i}");
        var line = CSV.FormatLine(values, eol: false);

        var parsed = CSV.ParseLine(line);
        Assert.AreEqual(columnCount, parsed.Length);
    }

    #endregion
}
