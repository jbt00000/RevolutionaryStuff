using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Diagnostics;

namespace RevolutionaryStuff.Core.Tests
{
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
            var tfn = Stuff.GetTempFileName(MimeType.Application.SpreadSheet.Xlsx.PrimaryFileExtension);
            Trace.WriteLine($"create {tfn}");
            ds.ToSpreadSheet(tfn);
        }
    }
}
