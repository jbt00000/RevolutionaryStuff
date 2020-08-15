using RevolutionaryStuff.Core.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;

namespace RevolutionaryStuff.Core
{
    public static class SpreadsheetHelpers
    {
        private static class CommonNamespaces
        {
            public const string SpreadsheetMain = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            public const string Relationships = "http://schemas.openxmlformats.org/package/2006/relationships";
            public const string WorkbookRelationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        }

        private static class RelationshipTypeNames
        {
            public const string Styles = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles";
            public const string Theme = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme";
            public const string Worksheet = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet";
            public const string SharedStrings = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings";
        }

        private static class NodeNames
        {
            public static class WorkbookNodes
            {
                public const string Sheets = "sheets";
                public const string Sheet = "sheet";
            }

            public static class RelsNodes
            {
                public const string Relationships = "Relationships";
                public const string Relationship = "Relationship";
            }

            public static class WorksheetNodes
            {
                public const string WorkSheet = "worksheet";
                public const string SheetViews = "sheetViews";
                public const string SheetView = "sheetView";
                public const string Pane = "pane";
                public const string Selection = "selection";
                public const string SheetData = "sheetData";
                public const string Row = "row";
                public const string Cell = "c";
                public const string CellValue = "v";
                public const string AutoFilter = "autoFilter";
            }
        }

        public static void ToSpreadSheet(this DataSet ds, Stream outputStream, Stream templateSpreadsheetStream = null)
        {
            var outFn = FileSystemHelpers.GetTempFileName(MimeType.Application.SpreadSheet.Xlsx.PrimaryFileExtension);
            ds.ToSpreadSheet(outFn, templateSpreadsheetStream);
            outputStream.CopyFrom(outFn);
        }

        public static void ToSpreadSheet(this DataSet ds, string outputPath, Stream templateSpreadsheetStream=null)
        {
            Requires.NonNull(ds, nameof(ds));
            Requires.Text(outputPath, nameof(outputPath));
            templateSpreadsheetStream = templateSpreadsheetStream ?? ResourceHelpers.GetEmbeddedResourceAsStream(Stuff.ThisAssembly, "Template.xlsx");
            Requires.ReadableStreamArg(templateSpreadsheetStream, nameof(templateSpreadsheetStream));

            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);
            try
            {
                var tfn = FileSystemHelpers.GetTempFileName(MimeType.Application.SpreadSheet.Xlsx.PrimaryFileExtension, dir);
                templateSpreadsheetStream.CopyTo(tfn);
                var workDir = Path.Combine(dir, "t");
                ZipFile.ExtractToDirectory(tfn, workDir);
                var sharedStringsPath = Path.Combine(workDir, "xl", "sharedStrings.xml");
                var indexBySharedString = LoadSharedStrings(sharedStringsPath);

                var itemsByType = new MultipleValueDictionary<string, Item>();
                itemsByType.Add(RelationshipTypeNames.SharedStrings, new Item("sharedStrings.xml"));

                var items = new List<Item>();
                foreach (DataTable dt in ds.Tables)
                {
                    var fn = RegexHelpers.Common.NonWordChars.Replace(dt.TableName, " ").ToUpperCamelCase();
                    var sheetRelPath = Path.Combine("xl", "worksheets", $"{fn}.xml");
                    var sheetPath = Path.Combine(workDir, sheetRelPath);
                    SaveSheet(sheetPath, indexBySharedString, dt);
                    itemsByType.Add(RelationshipTypeNames.Worksheet, new Item(Path.Combine("worksheets", $"{fn}.xml")) { Name = dt.TableName });
                }

                SaveSharedStrings(sharedStringsPath, indexBySharedString);

                SaveRels(Path.Combine(workDir, "xl", "_rels", "workbook.xml.rels"), itemsByType);
                SaveWorkbook(Path.Combine(workDir, "xl", "workbook.xml"), itemsByType);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                FileSystemHelpers.FileTryDelete(outputPath);
                ZipFile.CreateFromDirectory(workDir, outputPath);
                Stuff.Noop(outputPath);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        private static void SaveWorkbook(string relsPath, MultipleValueDictionary<string, Item> itemsByType)
        {
            var doc = new XmlDocument();
            doc.Load(relsPath);
            var mgr = new XmlNamespaceManager(doc.NameTable);
            var r = doc.GetPrefixOfNamespace(CommonNamespaces.WorkbookRelationships);
            Stuff.Noop(r);
            mgr.AddNamespace("z", CommonNamespaces.SpreadsheetMain);
            mgr.AddNamespace("r", CommonNamespaces.WorkbookRelationships);
            var sheetsNode = (XmlElement) doc.DocumentElement.SelectSingleNode($"//z:{NodeNames.WorkbookNodes.Sheets}", mgr);
            foreach (var item in itemsByType[RelationshipTypeNames.Worksheet].Where(z => z.IsNew))
            {
                var el = doc.CreateElement(NodeNames.WorkbookNodes.Sheet, CommonNamespaces.SpreadsheetMain);
                el.Attributes.Append(doc.CreateAttributeWithValue("name", null, item.Name));
                el.Attributes.Append(doc.CreateAttributeWithValue("sheetId", null, (sheetsNode.ChildNodes.Count + 1).ToString()));
                el.Attributes.Append(doc.CreateAttributeWithValue("id", CommonNamespaces.WorkbookRelationships, item.Id, "r"));
                sheetsNode.AppendChild(el);
            }
            doc.Save(relsPath);
        }

        private static void SaveRels(string relsPath, MultipleValueDictionary<string, Item> itemsByType)
        {
            var doc = new XmlDocument();
            doc.Load(relsPath);
            var relsNode = doc.DocumentElement;
            foreach (var relNode in relsNode.ChildNodes.OfType<XmlElement>())
            {
                if (relNode.NamespaceURI != CommonNamespaces.Relationships || relNode.LocalName != NodeNames.RelsNodes.Relationship) continue;
                var id = relNode.GetAttribute("Id");
                var type = relNode.GetAttribute("Type");
                var target = relNode.GetAttribute("Target");
                foreach (var item in itemsByType[type])
                {
                    if (0 == string.Compare(item.Target, target, true))
                    {
                        item.Id = id;
                        break;
                    }
                }
            }
            foreach (var kvp in itemsByType.AtomEnumerable.Where(z=>z.Value.Id==null))
            {
                var item = kvp.Value;
                item.Id = Item.CreateId();
                item.IsNew = true;
                var el = (XmlElement)doc.CreateNode(XmlNodeType.Element, NodeNames.RelsNodes.Relationship, CommonNamespaces.Relationships);
                el.Attributes.Append(doc.CreateAttributeWithValue("Id", null, item.Id));
                el.Attributes.Append(doc.CreateAttributeWithValue("Type", null, kvp.Key));
                el.Attributes.Append(doc.CreateAttributeWithValue("Target", null, item.Target));
                relsNode.AppendChild(el);
            }
            doc.Save(relsPath);
        }

        private class Item
        {
            public static string CreateId()
                => $"rId{Stuff.RandomWithRandomSeed.Next()}";

            public string Target;
            public string Id;
            public string Name;
            public bool IsNew;

            public Item(string path, string id=null)
            {
                Target = path.Replace("\\", "/");
                Id = id;
            }
        }

        private static void SaveSheet(string path, IDictionary<string, int> indexBySharedString, DataTable dt)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var writer = XmlWriter.Create(path))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement(NodeNames.WorksheetNodes.WorkSheet, CommonNamespaces.SpreadsheetMain);
                {
                    writer.WriteStartElement(NodeNames.WorksheetNodes.SheetViews, CommonNamespaces.SpreadsheetMain);
                    {
                        writer.WriteStartElement(NodeNames.WorksheetNodes.SheetView, CommonNamespaces.SpreadsheetMain);
                        writer.WriteAttributeString("tabSelected", "1");
                        writer.WriteAttributeString("workbookViewId", "0");
                        {
                            writer.WriteElement(NodeNames.WorksheetNodes.Pane, CommonNamespaces.SpreadsheetMain, null, new { ySplit = 1, topLeftCell = "A2", activePane = "bottomLeft", state = "frozen" });
                            writer.WriteElement(NodeNames.WorksheetNodes.Selection, CommonNamespaces.SpreadsheetMain, null, new { pane = "bottomLeft" });
                        }
                        writer.WriteEndElement(); //SheetView
                    }
                    writer.WriteEndElement(); //SheetViews
                    writer.WriteStartElement(NodeNames.WorksheetNodes.SheetData, CommonNamespaces.SpreadsheetMain);
                    {
                        WriteTableHeaders(writer, dt.Columns, indexBySharedString);
                        int rowNum = 1;
                        foreach (DataRow row in dt.Rows)
                        {
                            WriteRow(writer, row, indexBySharedString, rowNum++);
                        }
                    }
                    writer.WriteEndElement(); //sheetdata
                    writer.WriteElement(NodeNames.WorksheetNodes.AutoFilter, CommonNamespaces.SpreadsheetMain, null, new { @ref = CreateCellReference(0, 0, dt.Columns.Count - 1, dt.Rows.Count) });
                }
                writer.WriteEndElement(); //worksheet
                writer.WriteEndDocument();
            }
        }

        private static void WriteRow(XmlWriter writer, DataRow row, IDictionary<string, int> indexBySharedString, int absRowNum)
        {
            writer.WriteStartElement(NodeNames.WorksheetNodes.Row, CommonNamespaces.SpreadsheetMain);
            writer.WriteAttributeString("r", (1+absRowNum).ToString());
            int colNum = 0;
            foreach (var val in row.ItemArray)
            {
                WriteCell(writer, colNum++, absRowNum, val, indexBySharedString);
            }
            writer.WriteEndElement(); //row
        }

        private static void WriteTableHeaders(XmlWriter writer, DataColumnCollection cols, IDictionary<string, int> indexBySharedString)
        {
            writer.WriteStartElement(NodeNames.WorksheetNodes.Row, CommonNamespaces.SpreadsheetMain);
            writer.WriteAttributeString("r", "1");
            foreach (DataColumn col in cols)
            {
                WriteCell(writer, col.Ordinal, 0, col.ColumnName, indexBySharedString, 14);
            }
            writer.WriteEndElement(); //row
        }

        private static void WriteCell(XmlWriter writer, int colNum, int rowNum, object val, IDictionary<string, int> indexBySharedString, int? style=null)
        {
            string cellType;
            string cellValue;

            if (val == null || val == DBNull.Value) val = "";
            var t = val.GetType();
            if (t.IsNumber())
            {
                cellValue = val.ToString();
                cellType = "n";
            }
            else if (t == typeof(bool))
            {
                cellValue = (bool)val ? "1" : "0";
                cellType = "b";
            }
            else if (t == typeof(DateTime))
            {
                //                cell.CellValue = new CellValue(((DateTime)val).Date.ToOADate().ToString());
                //                cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Date);
                cellValue = ((DateTime)val).Date.ToShortDateString();
                cellType = "str";
            }
            else
            {
                cellValue = FindOrCreateSharedString(indexBySharedString, Stuff.ObjectToString(val)).ToString();
                cellType = "s";
            }


            writer.WriteStartElement(NodeNames.WorksheetNodes.Cell, CommonNamespaces.SpreadsheetMain);
            writer.WriteAttributeString("r", CreateCellReference(colNum, rowNum));
            if (style != null)
            {
                writer.WriteAttributeString("s", style.ToString());
            }
            writer.WriteAttributeString("t", cellType);
            writer.WriteElement(NodeNames.WorksheetNodes.CellValue, CommonNamespaces.SpreadsheetMain, cellValue);
            writer.WriteEndElement(); //cell
        }

        public static string CreateCellReference(int colStart, int rowStart, int colEnd, int rowEnd)
        {
            return CreateCellReference(colStart, rowStart) + ":" + CreateCellReference(colEnd, rowEnd);
        }

        public static string CreateCellReference(int colNum, int rowNum)
        {
            var cr = "";
            for (;;)
            {
                cr = ((char)('A' + colNum % 26)) + cr;
                if (colNum < 26) break;
                colNum = colNum / 26 - 1;
            }
            cr = cr + (rowNum + 1).ToString();
            return cr;
        }

        internal static int FindOrCreateSharedString(this IDictionary<string, int> indexBySharedString, string s)
        {
            int pos;
            if (!indexBySharedString.TryGetValue(s, out pos))
            {
                pos = indexBySharedString.Count;
                indexBySharedString[s] = pos;
            }
            return pos;
        }

        private static IDictionary<string, int> LoadSharedStrings(string sharedStringsPath)
        {
            Requires.Text(sharedStringsPath, nameof(sharedStringsPath));

            using (var st = File.Exists(sharedStringsPath) ? (Stream) File.OpenRead(sharedStringsPath) : new MemoryStream())
            {
                return LoadSharedStrings(st);
            }
        }

        internal static IDictionary<string, int> LoadSharedStrings(Stream st)
        {
            Requires.ReadableStreamArg(st, nameof(st));

            var d = new Dictionary<string, int>();
            if (st.Length>0)
            {
                var reader = XmlReader.Create(st, new XmlReaderSettings { CloseInput = false });
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "t" && reader.NamespaceURI == CommonNamespaces.SpreadsheetMain)
                    {
                        for (; reader.Read();)
                        {
                            if (reader.NodeType == XmlNodeType.Text)
                            {
                                d[reader.Value] = d.Count;
                                break;
                            }
                        }
                    }
                }
            }
            return d;
        }

        private static void SaveSharedStrings(string sharedStringsPath, IDictionary<string, int> sharedStrings)
        {
            Requires.Text(sharedStringsPath, nameof(sharedStringsPath));
            using (var st = File.Create(sharedStringsPath))
            {
                SaveSharedStrings(st, sharedStrings);
            }
        }

        internal static void SaveSharedStrings(Stream st, IDictionary<string, int> sharedStrings=null)
        {
            Requires.WriteableStreamArg(st, nameof(st));
            sharedStrings = sharedStrings ?? new Dictionary<string, int>();

            using (var writer = XmlWriter.Create(st))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("sst", CommonNamespaces.SpreadsheetMain);
                writer.WriteAttributeString("count", sharedStrings.Count.ToString());
                writer.WriteAttributeString("uniqueCount", sharedStrings.Count.ToString());
                var lastNum = -1;
                foreach (var kvp in sharedStrings.ToList().OrderBy(z => z.Value))
                {
                    if (++lastNum != kvp.Value) throw new Exception("Shared strings not in order or zero based");
                    writer.WriteStartElement("si", CommonNamespaces.SpreadsheetMain);
                    writer.WriteStartElement("t", CommonNamespaces.SpreadsheetMain);
                    writer.WriteString(kvp.Key);
                    writer.WriteEndElement();//t                    
                    writer.WriteEndElement();//si                    
                }
                writer.WriteEndElement();//sst
                writer.WriteEndDocument();
                writer.Flush();
            }
        }
    }
}
