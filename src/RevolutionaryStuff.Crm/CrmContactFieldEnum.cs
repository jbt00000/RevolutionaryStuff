using System.Runtime.Serialization;

namespace RevolutionaryStuff.Crm;

public enum CrmContactFieldEnum
{
    [EnumMember(Value = CrmJointFieldNames.ItemName)]
    Name,

    [EnumMember(Value = CrmJointFieldNames.Email)]
    Email,

    [EnumMember(Value = CrmJointFieldNames.Phone)]
    Phone,

    [EnumMember(Value = CrmJointFieldNames.Address)]
    Address,
}
