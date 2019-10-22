using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Mergers
{
    public class MergeOutputSettings : IValidate
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("contentDispositionFilename")]
        public string ContentDispositionFilename { get; set; }

        [JsonProperty("contentDispositionType")]
        public string ContentDispositionType { get; set; }

        public virtual void Validate()
        {
            Requires.Text(ContentDispositionFilename, nameof(ContentDispositionFilename));
        }
    }
}
