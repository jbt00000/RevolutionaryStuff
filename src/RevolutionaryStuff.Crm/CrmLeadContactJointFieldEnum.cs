using System.Runtime.Serialization;

namespace RevolutionaryStuff.Crm;

public enum CrmLeadContactJointFieldEnum
{
    [EnumMember(Value = CrmJointFieldNames.ItemName)]
    Name,

    [EnumMember(Value = CrmJointFieldNames.Email)]
    Email,

    [EnumMember(Value = CrmJointFieldNames.Phone)]
    Phone,
}
