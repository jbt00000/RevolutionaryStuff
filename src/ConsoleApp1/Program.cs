using RevolutionaryStuff.Core;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private static Task Yo()
        {
            return Task.Run(async () => {
                await Task.Delay(TimeSpan.FromSeconds(10));
                throw new NotNowException();
            });
        }

        private static async Task<string> Mama()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            return "Mama";
        }

        private static async Task<string> Drama()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            throw new Exception("loads going on");
        }

        static void Main(string[] args)
        {
            try
            {
                Task.CompletedTask.ExecuteSynchronously();
                Yo().ExecuteSynchronously();
                //                Yo().ExecuteSynchronously();
                Trace.WriteLine(Mama().ExecuteSynchronously());
                Trace.WriteLine(Drama().ExecuteSynchronously());


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
