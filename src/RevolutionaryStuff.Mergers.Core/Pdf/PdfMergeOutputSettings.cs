using Newtonsoft.Json;

namespace RevolutionaryStuff.Mergers.Pdf
{
    public class PdfMergeOutputSettings : MergeOutputSettings
    {
        [JsonProperty("needAppearances")]
        public bool? NeedAppearances { get; set; }
    }
}
