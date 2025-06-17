using System.Runtime.Serialization;

public enum BoardStateEnum
{
    Unknown = 0,

    [EnumMember(Value = "active")]
    Active,

    [EnumMember(Value = "all")]
    All,

    [EnumMember(Value = "archived")]
    Archived,

    [EnumMember(Value = "deleted")]
    Deleted
}
