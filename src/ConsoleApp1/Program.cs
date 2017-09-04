using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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


        private class JsonTemplate
        {
            public readonly string Template;
            public readonly string StringFormat;
            public readonly IList<string> Fieldnames;
            private static readonly Regex TemplateParseExpr = new Regex("@(\\w+)|@\\((\\w+)\\)", RegexOptions.Compiled|RegexOptions.Singleline);

            public string Format(object[] args)
                => string.Format(StringFormat, args);
                
            public JsonTemplate(string template)
            {
                var fieldPosByFieldName = new Dictionary<string, int>(Comparers.CaseInsensitiveStringComparer);
                Template = template = (template ?? "");
                template = template.Replace("{","{{").Replace("}", "}}");
                StringFormat = "";
                int startAt = 0;
                Again:
                var m = TemplateParseExpr.Match(template, startAt);
                if (m.Success)
                {
                    StringFormat += template.Substring(startAt, m.Index - startAt);
                    if (m.Index > 0 && template[m.Index - 1] == '@')
                    {
                        StringFormat += m.Value.Substring(1);
                    }
                    else
                    {
                        var fieldName = StringHelpers.Coalesce(m.Groups[1].Value, m.Groups[2].Value);
                        int pos;
                        if (!fieldPosByFieldName.TryGetValue(fieldName, out pos))
                        {
                            pos = fieldPosByFieldName.Count;
                            fieldPosByFieldName[fieldName] = pos;
                        }
                        StringFormat += "{" + pos.ToString() + "}";
                    }
                    startAt = m.Index + m.Length;
                    goto Again;
                }
                else
                {
                    StringFormat += template.Substring(startAt);
                }
                Fieldnames = fieldPosByFieldName.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList().AsReadOnly();
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Trace.WriteLine(DateTime.Now.ToRfc7231());

                var t = new JsonTemplate(@"{
	""FieldA"" : @ColA,
	""FieldB"" : [@ColB, @ColA, 123, ""m@@test.com"", @ColD, ""hello@(fdsa)""]
}");

                Stuff.Noop(t);
                return;

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
