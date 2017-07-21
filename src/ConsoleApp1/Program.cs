using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var ds = new DataSet();
                var dt = new DataTable("Sheet2");
                ds.Tables.Add(dt);
                dt.Columns.Add("a");
                dt.Columns.Add("b");
                dt.Columns.Add("c");
                dt.Rows.Add("a1", "a2", "a3");
                dt = new DataTable("SheetR and b");
                ds.Tables.Add(dt);
                dt.Columns.Add("a");
                dt.Columns.Add("b");
                for (int z = 0; z < 10000; ++z)
                {
                    dt.Rows.Add(z, Stuff.Random.Next());
                }
                var tfn = Stuff.GetTempFileName(MimeType.Application.SpreadSheet.Xlsx.PrimaryFileExtension);
                Trace.WriteLine($"create {tfn}");
                ds.ToSpreadSheet(tfn);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
