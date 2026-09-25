using System.Runtime.Serialization;
using System.Text.Json.Serialization;

public enum BoardStateEnum
{
    Unknown = 0,

    [JsonStringEnumMemberName("active")]
    Active,

    [JsonStringEnumMemberName("all")]
    All,

    [JsonStringEnumMemberName("archived")]
    Archived,

    [JsonStringEnumMemberName("deleted")]
    Deleted
}
