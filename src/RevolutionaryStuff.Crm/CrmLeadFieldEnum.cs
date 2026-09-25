using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Crm;

public enum CrmLeadFieldEnum
{
    [JsonStringEnumMemberName(CrmJointFieldNames.ItemName)]
    Name,

    [JsonStringEnumMemberName(CrmJointFieldNames.Email)]
    Email,

    [JsonStringEnumMemberName(CrmJointFieldNames.Phone)]
    Phone,

    [JsonStringEnumMemberName(CrmJointFieldNames.Address)]
    Address,

    [JsonStringEnumMemberName("firstName")]
    FirstName,

    [JsonStringEnumMemberName("lastName")]
    LastName,
}
