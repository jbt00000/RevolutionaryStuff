using Newtonsoft.Json;

namespace RevolutionaryStuff.Mergers
{
    public class FieldData
    {
        [JsonProperty("k")]
        public string FieldKey { get; set; }

        [JsonProperty("v")]
        public object FieldVal { get; set; }

        [JsonProperty("cs")]
        public bool? FieldNameIsCaseSensitive { get; set; }

        [JsonProperty("ss")]
        public SerializationSettings SerializationSettings { get; set; }

        public FieldData() { }

        public FieldData(string key, object val)
        {
            FieldKey = key;
            FieldVal = val;
        }
    }
}
