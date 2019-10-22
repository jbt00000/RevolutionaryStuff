using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.Mergers.Pdf
{
    public class PdfMerger : IPdfMerger
    {
        private readonly IOptions<Config> ConfigOptions;

        public class Config
        {
            public const string ConfigSectionName = "PdfMergerConfig";

            /// <summary>
            /// Url of the entrypoint for the PDF Capability
            /// </summary>
            public Uri Url { get; set; }
        }

        public PdfMerger(IOptions<Config> configOptions)
        {
            ConfigOptions = configOptions;
        }

        async Task<Stream> IPdfMerger.MergeAsync(PdfMergeInputs mergeInputs)
        {
            Requires.Valid(mergeInputs, nameof(mergeInputs));

            var c = new MultipartFormDataContent();
            if (mergeInputs.Template.TemplateStream != null)
            {
                var sc = new StreamContent(mergeInputs.Template.TemplateStream);
                mergeInputs.Template.MultipartName = mergeInputs.Template.MultipartName ?? "templateStream";
                sc.Headers.Add(WebHelpers.HeaderStrings.ContentType, MimeType.Application.Pdf.PrimaryContentType);
                c.Add(sc, mergeInputs.Template.MultipartName, "the.pdf");
            }
            var json = mergeInputs.ToJson();
            c.Add(WebHelpers.CreateJsonContent(json), MergerHelpers.MergeInputsMultipartName);
            using (var hc = new HttpClient())
            {
                var resp = await hc.PostAsync(ConfigOptions.Value.Url, c);
                return await resp.Content.ReadAsStreamAsync();
            }
        }
    }
}
