using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Crm;

public enum CrmContactFieldEnum
{
    [JsonStringEnumMemberName(CrmJointFieldNames.ItemName)]
    Name,

    [JsonStringEnumMemberName(CrmJointFieldNames.Email)]
    Email,

    [JsonStringEnumMemberName(CrmJointFieldNames.Phone)]
    Phone,

    [JsonStringEnumMemberName(CrmJointFieldNames.Address)]
    Address,
}
