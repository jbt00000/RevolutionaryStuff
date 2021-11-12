using System.Data;
using System.IO;
using HtmlAgilityPack;

namespace RevolutionaryStuff.ETL;

public class LoadRowsFromHtmlSettings : LoadRowsSettings
{
    public string TableId { get; set; }
    public string TableXPath { get; set; }
}

public static class HtmlTableFileFormatHelpers
{
    public static DataTable Load(Stream st, LoadRowsFromHtmlSettings settings)
    {
        var doc = new HtmlDocument();
        doc.Load(st);
        string xpath;
        if (settings.TableXPath != null)
        {
            xpath = settings.TableXPath;
        }
        else if (settings.TableId != null)
        {
            xpath = $"//table[id='{settings.TableId}']";
        }
        else
        {
            xpath = $"//table";
        }
        DataTable dt = null;
        var tables = doc.DocumentNode.SelectNodes(xpath);
        if (tables != null && tables.Count > 0)
        {
            dt = new DataTable();
            var table = tables[0];
            var headerRow = table.SelectSingleNode("descendant::tr[1]");
            foreach (var cell in headerRow.ChildNodes)
            {
                if (cell.NodeType != HtmlNodeType.Element) continue;
                if (cell.Name == "td" || cell.Name == "th")
                {
                    dt.Columns.Add(cell.InnerText.Trim());
                }
            }
            int skipRows = 0;
            bool skipThRows = false;
            var tableBodyRows = table.SelectNodes("descendant::tbody/tr");
            if (tableBodyRows == null || tableBodyRows.Count == 0)
            {
                tableBodyRows = table.SelectNodes("descendant::tr");
                skipRows = 1;
                skipThRows = true;
            }
            int rowNum = 0;
            var items = new List<object>(dt.Columns.Count);
            foreach (var tr in tableBodyRows)
            {
                if (rowNum++ < skipRows) continue;
                items.Clear();
                foreach (var cell in tr.ChildNodes)
                {
                    if (cell.NodeType != HtmlNodeType.Element) continue;
                    if (skipThRows && items.Count == 0 && cell.Name == "th") goto NextRow;
                    var val = cell.InnerText;
                    items.Add(val);
                }
                dt.Rows.Add(items.ToArray());
NextRow:
                Stuff.Noop();
            }
        }
        return dt;
    }
}
