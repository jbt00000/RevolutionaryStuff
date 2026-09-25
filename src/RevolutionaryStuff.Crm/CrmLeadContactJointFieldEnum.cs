using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Crm;

public enum CrmLeadContactJointFieldEnum
{
    [JsonStringEnumMemberName(CrmJointFieldNames.ItemName)]
    Name,

    [JsonStringEnumMemberName(CrmJointFieldNames.Email)]
    Email,

    [JsonStringEnumMemberName(CrmJointFieldNames.Phone)]
    Phone,
}
