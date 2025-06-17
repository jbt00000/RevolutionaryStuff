using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Crm.MondayCom;

public class MondayColumn
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonIgnore]
    public ColumnTypeEnum ColumnType
        => Parse.ParseEnumWithEnumMemberValues<ColumnTypeEnum>(RawColumnType, false, ColumnTypeEnum.Unsupported);

    [JsonPropertyName("type")]
    public string? RawColumnType { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("settings_str")]
    public string? SettingsStr { get; set; }

    public string? GetLookupLabel(string? proposedLabel, string? missing = null)
        => StatusSettings?.GetLabelByName(proposedLabel)?.Label ?? DropdownSettings?.GetLabelByName(proposedLabel)?.Name ?? missing;

    [JsonIgnore]
    public MondayStatusColumnSettings StatusSettings
    {
        get
        {
            if (field == null && !string.IsNullOrEmpty(SettingsStr))
            {
                switch (ColumnType)
                {
                    case ColumnTypeEnum.Status:
                        field = JsonSerializer.Deserialize<MondayStatusColumnSettings>(SettingsStr);
                        break;
                }
            }
            return field;
        }
    }

    [JsonIgnore]
    public MondayDropdownColumnSettings? DropdownSettings
    {
        get
        {
            if (field == null && !string.IsNullOrEmpty(SettingsStr))
            {
                switch (ColumnType)
                {
                    case ColumnTypeEnum.Dropdown:
                        field = JsonSerializer.Deserialize<MondayDropdownColumnSettings>(SettingsStr);
                        break;
                }
            }
            return field;
        }
    }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    public override string ToString()
        => $"[{Title}({RawColumnType})] - {Id}";
}
