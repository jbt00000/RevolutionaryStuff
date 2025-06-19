using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Crm.MondayCom;

public class MondayBoard
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("state")]
    public string? State_FSO { get; set; }

    [JsonIgnore]
    public BoardStateEnum State
        => Parse.ParseEnumWithEnumMemberValues<BoardStateEnum>(State_FSO, false, BoardStateEnum.Unknown);

    [JsonPropertyName("type")]
    public string? Type_FSO { get; set; }

    [JsonIgnore]
    public BoardTypeEnum Type
        => Parse.ParseEnumWithEnumMemberValues<BoardTypeEnum>(Type_FSO, false, BoardTypeEnum.Unknown);

    [JsonPropertyName("board_kind ")]
    public string? BoardKind_FSO { get; set; }

    [JsonIgnore]
    public BoardKindEnum BoardKind
        => Parse.ParseEnumWithEnumMemberValues<BoardKindEnum>(BoardKind_FSO, false, BoardKindEnum.Unknown);

    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("owner")]
    public MondayUser? Owner { get; set; }

    [JsonPropertyName("groups")]
    public List<MondayGroup>? Groups { get; set; }

    [JsonPropertyName("top_group")]
    public MondayGroup? TopGroup { get; set; }

    [JsonPropertyName("columns")]
    public List<MondayColumn>? Columns { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    public override string ToString()
        => $"[{Name}] - {Id}";

    public MondayGroup? GetGroupByName(string? name)
        => Groups.NullSafeEnumerable().FirstOrDefault(z => 0 == StringHelpers.CompareIgnoreCase(z.Title, name));

    public MondayColumn? GetColumnByName(string? name)
        => Columns.NullSafeEnumerable().FirstOrDefault(z => 0 == StringHelpers.CompareIgnoreCase(z.Title, name));
}
