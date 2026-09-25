using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Crm;

public enum CrmRecordTypeEnum
{
    [JsonStringEnumMemberName("contact")]
    Contact,

    [JsonStringEnumMemberName("lead")]
    Lead,
}
