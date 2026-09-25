using System.Runtime.Serialization;
using System.Text.Json.Serialization;

public enum BoardTypeEnum
{
    Unknown = 0,

    [JsonStringEnumMemberName("board")]
    Board,

    [JsonStringEnumMemberName("custom_object")]
    CustomObject,

    [JsonStringEnumMemberName("document")]
    Document,

    [JsonStringEnumMemberName("sub_items_board")]
    SubItemsBoard
}
