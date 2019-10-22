using Newtonsoft.Json;

namespace RevolutionaryStuff.Mergers
{
    public class SerializationSettings
    {
        [JsonProperty("dateAsTextFormat")]
        public string DateAsTextFormat { get; set; }

        [JsonProperty("trueAsText")]
        public string BoolTrueAsText { get; set; }

        [JsonProperty("falseAsText")]
        public string BoolFalseAsText { get; set; }

        [JsonProperty("nullAsText")]
        public string NullAsText { get; set; }
    }
}
