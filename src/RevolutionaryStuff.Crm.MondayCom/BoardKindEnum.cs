using System.Runtime.Serialization;
using System.Text.Json.Serialization;

public enum BoardKindEnum
{
    Unknown = 0,

    [JsonStringEnumMemberName("private")]
    Private,

    [JsonStringEnumMemberName("public")]
    Public,

    [JsonStringEnumMemberName("share")]
    Share,
}
