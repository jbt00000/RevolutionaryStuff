using System.Runtime.Serialization;
using System.Text.Json.Serialization;

public enum ColumnTypeEnum
{
    [JsonStringEnumMemberName("auto_number")]
    AutoNumber,

    [JsonStringEnumMemberName("board_relation")]
    BoardRelation,

    [JsonStringEnumMemberName("button")]
    Button,

    [JsonStringEnumMemberName("checkbox")]
    Checkbox,

    [JsonStringEnumMemberName("color_picker")]
    ColorPicker,

    [JsonStringEnumMemberName("country")]
    Country,

    [JsonStringEnumMemberName("creation_log")]
    CreationLog,

    [JsonStringEnumMemberName("date")]
    Date,

    [JsonStringEnumMemberName("dependency")]
    Dependency,

    [JsonStringEnumMemberName("doc")]
    Doc,

    [JsonStringEnumMemberName("dropdown")]
    Dropdown,

    [JsonStringEnumMemberName("email")]
    Email,

    [JsonStringEnumMemberName("file")]
    File,

    [JsonStringEnumMemberName("formula")]
    Formula,

    [JsonStringEnumMemberName("hour")]
    Hour,

    [JsonStringEnumMemberName("item_assignees")]
    ItemAssignees,

    [JsonStringEnumMemberName("item_id")]
    ItemId,

    [JsonStringEnumMemberName("last_updated")]
    LastUpdated,

    [JsonStringEnumMemberName("link")]
    Link,

    [JsonStringEnumMemberName("location")]
    Location,

    [JsonStringEnumMemberName("long_text")]
    LongText,

    [JsonStringEnumMemberName("mirror")]
    Mirror,

    [JsonStringEnumMemberName("name")]
    Name,

    [JsonStringEnumMemberName("numbers")]
    Numbers,

    [JsonStringEnumMemberName("people")]
    People,

    [JsonStringEnumMemberName("phone")]
    Phone,

    [JsonStringEnumMemberName("progress")]
    Progress,

    [JsonStringEnumMemberName("rating")]
    Rating,

    [JsonStringEnumMemberName("status")]
    Status,

    [JsonStringEnumMemberName("subtasks")]
    Subtasks,

    [JsonStringEnumMemberName("tags")]
    Tags,

    [JsonStringEnumMemberName("team")]
    Team,

    [JsonStringEnumMemberName("text")]
    Text,

    [JsonStringEnumMemberName("timeline")]
    Timeline,

    [JsonStringEnumMemberName("time_tracking")]
    TimeTracking,

    [JsonStringEnumMemberName("unsupported")]
    Unsupported,

    [JsonStringEnumMemberName("vote")]
    Vote,

    [JsonStringEnumMemberName("week")]
    Week,

    [JsonStringEnumMemberName("world_clock")]
    WorldClock
}
