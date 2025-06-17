using System.Text.Json;
using System.Text.Json.Serialization;

public class MondayDropdownColumnSettings
{
    [JsonExtensionData]
    public IDictionary<string, JsonElement> AdditionalData { get; set; }
    [JsonPropertyName("limit_select")]
    public bool LimitSelect { get; set; }
    [JsonPropertyName("hide_footer")]
    public bool HideFooter { get; set; }
    [JsonPropertyName("labels")]
    public List<MondayDropdownLabel> Labels { get; set; } = [];
    public class MondayDropdownLabel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public MondayDropdownLabel? GetLabelByName(string labelName, bool ignoreCase = true)
    {
        foreach (var l in Labels.NullSafeEnumerable())
        {
            if (0 == string.Compare(labelName, l.Name, ignoreCase))
            {
                return l;
            }
        }
        return null;
    }

}

