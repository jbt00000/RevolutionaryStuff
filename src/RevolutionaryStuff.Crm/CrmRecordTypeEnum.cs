using System.Runtime.Serialization;

namespace RevolutionaryStuff.Crm;

public enum CrmRecordTypeEnum
{
    [EnumMember(Value = "contact")]
    Contact,

    [EnumMember(Value = "lead")]
    Lead,
}
