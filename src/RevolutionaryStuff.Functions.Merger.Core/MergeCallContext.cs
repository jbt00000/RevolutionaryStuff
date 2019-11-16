using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Functions.Mergers;
using RevolutionaryStuff.Mergers;

namespace RevolutionaryStuff.Functions.Merger.Core
{
    public class MergeCallContext<TMI> : BaseDisposable where TMI : IMergeInputs
    {
        public TMI Inputs { get; }
        public Stream TemplateStream { get; private set; }
        public string TemplateContentType { get; private set; }

        private MergeCallContext(TMI inputs)
        {
            Inputs = inputs;
        }

        private IDictionary<string, FieldData> FieldDataByCaseInsensitiveKeys = new Dictionary<string, FieldData>(Comparers.CaseInsensitiveStringComparer);
        private IDictionary<string, FieldData> FieldDataByCaseSensitiveKeys = new Dictionary<string, FieldData>();

        private void Compile()
        {
            if (Inputs.Data.FieldDatas != null)
            {
                foreach (var fd in Inputs.Data.FieldDatas)
                {
                    var d = fd.FieldNameIsCaseSensitive.GetValueOrDefault(Inputs.Data.FieldNamesAreCaseSensitive) ? FieldDataByCaseSensitiveKeys: FieldDataByCaseInsensitiveKeys;
                    d[fd.FieldKey] = fd;
                }            
            }
        }

        public bool TryGetFieldDataByKey(string key, out FieldData fd)
        {
            fd = null;
            return 
                FieldDataByCaseSensitiveKeys.TryGetValue(key, out fd) ||
                FieldDataByCaseInsensitiveKeys.TryGetValue(key, out fd);
        }

        public string ToString(FieldData fd)
        {
            var ss = fd.SerializationSettings ?? Inputs.Data.SerializationSettings ?? ServerMergerHelpers.DefaultSerializationSettings;
            if (fd.FieldVal == null)
            {
                return ss.NullAsText;
            }
            var t = fd.FieldVal.GetType();
            var v = fd.FieldVal;
            if (t == typeof(bool))
            {
                if (v.Equals(true))
                {
                    return ss.BoolTrueAsText;
                }
                else
                {
                    return ss.BoolFalseAsText;
                }
            }
            else if (t == typeof(DateTime))
            {
                return ((DateTime)v).ToString(ss.DateAsTextFormat);
            }
            return v.ToString();
        }

        public FileStreamResult CreateFileStreamResult(HttpRequest req, Stream st, MergeOutputSettings outputSettings)
        {
            var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            contentDisposition.SetHttpFileName(outputSettings.ContentDispositionFilename);
            req.HttpContext.Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
            return new FileStreamResult(st, outputSettings.ContentType);
        }

        public static async Task<MergeCallContext<TMI>> CreateAsync(HttpRequest req, ILogger log, Func<string, TMI> deserializer)
        {
            Requires.NonNull(req, nameof(req));
            Requires.NonNull(log, nameof(log));

            var webform = await req.ReadFormAsync();
            string json = webform[MergerHelpers.MergeInputsMultipartName];
            var c = new MergeCallContext<TMI>(deserializer(json));

            if (c.Inputs.Template.LocationUrl != null)
            {
                using (var client = new HttpClient())
                {
                    var resp = await client.GetAsync(c.Inputs.Template.LocationUrl);
                    if (!resp.IsSuccessStatusCode)
                    {
                        throw new ParameterizedMessageException("Problem downloading template from {templateUrl}", c.Inputs.Template.LocationUrl);
                    }
                    c.TemplateStream = new MemoryStream();
                    await resp.Content.CopyToAsync(c.TemplateStream);
                    c.TemplateStream.Position = 0;
                    c.TemplateContentType = resp.Headers.GetValues(WebHelpers.HeaderStrings.ContentType).FirstOrDefault();
                }
            }
            else if (c.Inputs.Template.MultipartName != null)
            {
                var file = webform.Files[c.Inputs.Template.MultipartName];
                if (file == null)
                {
                    throw new ParameterizedMessageException("Could not find {name} in webForm.Files", c.Inputs.Template.MultipartName);
                }
                c.TemplateStream = file.OpenReadStream();
                c.TemplateContentType = file.ContentType;
            }
            else
            {
                throw new Exception("Template must currently be either a LocationUrl or a MultiPart file");
            }
            c.Compile();
            return c;
        }
    }
}
