using System;
using System.IO;
using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Mergers
{
    public class TemplateInfo : IValidate
    {
        [JsonProperty("locationUrl")]
        public Uri LocationUrl { get; set; }

        [JsonProperty("multipartName")]
        public string MultipartName { get; set; }

        [JsonIgnore]
        public Stream TemplateStream { get; set; }

        public virtual void Validate()
        {
            Requires.ExactlyOneNonNull(LocationUrl, TemplateStream);
            if (TemplateStream != null)
            {
                Requires.ReadableStreamArg(TemplateStream, nameof(TemplateStream));
            }
        }
    }
}
