using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class SpreadsheetHelperTests
{
    [TestMethod]
    public void ToSpreadsheetTest()
    {
        var ds = new DataSet();
        var dt = new DataTable("T1");
        ds.Tables.Add(dt);
        dt.Columns.Add("a");
        dt.Columns.Add("b");
        dt.Columns.Add("c");
        dt.Rows.Add("a1", "a2", "a3");
        var tfn = FileSystemHelpers.GetTempFileName(MimeType.Application.SpreadSheet.MicrosoftExcelOpenXml.PrimaryFileExtension);
        Trace.WriteLine($"create {tfn}");
        ds.ToSpreadSheet(tfn);
    }

    [TestMethod]
    public void SaveSharedStringsEmptyCreate()
    {
        using var st = new MemoryStream();
        SpreadsheetHelpers.SaveSharedStrings(st);
        st.Position = 0;
        var d = SpreadsheetHelpers.LoadSharedStrings(st);
        Assert.AreEqual(0, d.Count);
    }

    [TestMethod]
    public void SaveSharedStringsManyCreate()
    {
        using var st = new MemoryStream();
        var sharedStrings = new Dictionary<string, int>();
        sharedStrings.FindOrCreateSharedString("Jason");
        sharedStrings.FindOrCreateSharedString("Bryce");
        sharedStrings.FindOrCreateSharedString("Thomas");
        for (var z = 0; z < 10000; ++z)
        {
            var num = Stuff.RandomWithRandomSeed.Next(1000000);
            var indexA = sharedStrings.FindOrCreateSharedString($"{z}");
            var indexB = sharedStrings.FindOrCreateSharedString($"{z}");
            Assert.AreEqual(indexA, indexB);
            sharedStrings.FindOrCreateSharedString($"{num}");
        }
        SpreadsheetHelpers.SaveSharedStrings(st, sharedStrings);
        st.Position = 0;
        var d = SpreadsheetHelpers.LoadSharedStrings(st);
        Assert.AreEqual(sharedStrings.Count, d.Count);
        foreach (var kvp in sharedStrings)
        {
            Assert.AreEqual(kvp.Value, d.FindOrCreateSharedString(kvp.Key));
        }
    }
}
