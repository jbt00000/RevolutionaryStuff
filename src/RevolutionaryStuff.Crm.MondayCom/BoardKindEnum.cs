using System.Runtime.Serialization;

public enum BoardKindEnum
{
    Unknown = 0,

    [EnumMember(Value = "private")]
    Private,

    [EnumMember(Value = "public")]
    Public,

    [EnumMember(Value = "share")]
    Share,
}
