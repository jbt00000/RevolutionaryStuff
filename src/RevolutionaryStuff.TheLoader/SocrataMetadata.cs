using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Database;

namespace RevolutionaryStuff.TheLoader
{
    public class SocrataMetadata
    {
        public Uri SocrataUrl { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Updated { get; set; }
        public string Category { get; set; }
        public string ODataUrl { get; set; }
        public Uri PageUrl { get; set; }
        public string TableName { get; set; }
        public string Owner { get; set; }
        public int? Size { get; set; }

        private static readonly Regex InitialStateJsonExpr = new Regex(@"var\s+initialState\s+=\s+{(.+?)}\s+;", RegexOptions.Compiled);

        private static string TrimPlus(string s)
        {
            s = s ?? "";
            s = RegexHelpers.Common.Whitespace.Replace(s, " ");
            s = s.Replace("&amp;", "&");
            return StringHelpers.TrimOrNull(s);
        }

        public static SocrataMetadata Fetch(Uri sourceUrl, string id)
        {
            var url = new Uri($"{sourceUrl.GetComponents(UriComponents.HostAndPort | UriComponents.SchemeAndServer, UriFormat.Unescaped)}/d/{id}");

            var client = new HttpClient();
            var resp = DelegateHelpers.CallAndRetryOnFailure(() => client.GetAsync(url).ExecuteSynchronously());
            var html = resp.Content.ReadAsStringAsync().ExecuteSynchronously();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            Match m;

            var nodes = doc.DocumentNode.SelectNodes("//script[@type=\"text/javascript\"]");
            string initialStateJson = null;
            foreach (HtmlAgilityPack.HtmlNode node in nodes)
            {
                m = InitialStateJsonExpr.Match(node.InnerText);
                if (!m.Success) continue;
                initialStateJson = "{" + m.Groups[1].Value + "}";
                break;
            }
            if (initialStateJson == null) return null;
            Socrata.Rootobject socrataRoot;
            try
            {
                socrataRoot = JsonConvert.DeserializeObject<Socrata.Rootobject>(initialStateJson);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }

            var md = new SocrataMetadata
            {
                Id = id,
                Title = socrataRoot.view.name,
                Description = TrimPlus(socrataRoot.view.description),
                Updated = socrataRoot.view.lastUpdatedAt,
                Category = socrataRoot.view.category,
                ODataUrl = socrataRoot.view.odataUrlV4,
                PageUrl = sourceUrl,
                Owner = socrataRoot.view.ownerName,
                SocrataUrl = url
            };
            md.TableName = (RegexHelpers.Common.NonWordChars.Replace(md.Title, " ").ToUpperCamelCase() + "_" + id.Replace("-", "")).TruncateWithMidlineEllipsis(SqlServerHelpers.MaxTableNameLength, "___");

            int size = 0;
            foreach (var col in socrataRoot.view.columns.Where(zz => zz.cachedContents != null))
            {
                size = Stuff.Max(size, col.cachedContents.non_null);
            }
            if (size > 0)
            {
                md.Size = size;
            }
            return md;
        }
    }
}
