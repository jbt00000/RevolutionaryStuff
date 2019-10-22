using System.Collections.Generic;
using Newtonsoft.Json;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Mergers
{
    public class MergeDataInfo : IValidate
    {
        [JsonProperty("fieldDatas")]
        public IList<FieldData> FieldDatas { get; set; }

        [JsonProperty("multipartName")]
        public string MergeFileMultipartName { get; set; }

        [JsonProperty("fieldNamesAreCaseSensitive")]
        public bool FieldNamesAreCaseSensitive { get; set; }

        [JsonProperty("serializationSettings")]
        public SerializationSettings SerializationSettings { get; set; }

        public virtual void Validate()
        {
        }
    }
}
