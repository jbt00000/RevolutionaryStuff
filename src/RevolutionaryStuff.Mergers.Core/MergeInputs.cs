using Newtonsoft.Json;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.Mergers
{
    public abstract class MergeInputs<TTemplateInfo, TMergeDataInfo, TMergeOutputSettings> : IMergeInputs
        where TTemplateInfo : TemplateInfo
        where TMergeDataInfo : MergeDataInfo
        where TMergeOutputSettings : MergeOutputSettings
    {
        public string ToJson() => Stuff.ToJson(this);

        [JsonProperty("template")]
        public TTemplateInfo Template { get; set; }

        [JsonProperty("data")]
        public TMergeDataInfo Data { get; set; }

        [JsonProperty("outputSettings")]
        public TMergeOutputSettings OutputSettings { get; set; }

        TemplateInfo IMergeInputs.Template => Template;

        MergeDataInfo IMergeInputs.Data => Data;

        MergeOutputSettings IMergeInputs.OutputSettings => OutputSettings;

        private bool TemplateRequired;

        protected MergeInputs(bool templateRequired)
        {
            TemplateRequired = templateRequired;
        }

        public virtual void Validate()
        {
            if (TemplateRequired)
            {
                Requires.Valid(Template, nameof(Template));
            }
            Requires.Valid(Data, nameof(Data));
            Requires.Valid(OutputSettings, nameof(OutputSettings), true);
        }
    }
}
