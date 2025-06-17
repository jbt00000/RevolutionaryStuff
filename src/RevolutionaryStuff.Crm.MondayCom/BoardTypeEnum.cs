using System.Runtime.Serialization;

public enum BoardTypeEnum
{
    Unknown = 0,

    [EnumMember(Value = "board")]
    Board,

    [EnumMember(Value = "custom_object")]
    CustomObject,

    [EnumMember(Value = "document")]
    Document,

    [EnumMember(Value = "sub_items_board")]
    SubItemsBoard
}
