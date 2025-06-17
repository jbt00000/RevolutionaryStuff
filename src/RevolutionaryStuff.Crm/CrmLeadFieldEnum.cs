using System.Runtime.Serialization;

namespace RevolutionaryStuff.Crm;

public enum CrmLeadFieldEnum
{
    [EnumMember(Value = CrmJointFieldNames.ItemName)]
    Name,

    [EnumMember(Value = CrmJointFieldNames.Email)]
    Email,

    [EnumMember(Value = CrmJointFieldNames.Phone)]
    Phone,

    [EnumMember(Value = CrmJointFieldNames.Address)]
    Address,

    [EnumMember(Value = "firstName")]
    FirstName,

    [EnumMember(Value = "lastName")]
    LastName,
}
