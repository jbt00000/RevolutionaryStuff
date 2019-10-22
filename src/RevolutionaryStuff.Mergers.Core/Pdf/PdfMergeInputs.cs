using Newtonsoft.Json;

namespace RevolutionaryStuff.Mergers.Pdf
{
    public class PdfMergeInputs : MergeInputs<TemplateInfo, MergeDataInfo, PdfMergeOutputSettings>
    {
        public static PdfMergeInputs CreateFromJson(string json)
            => JsonConvert.DeserializeObject<PdfMergeInputs>(json);

        public PdfMergeInputs()
            : base(true)
        { }
    }
}
