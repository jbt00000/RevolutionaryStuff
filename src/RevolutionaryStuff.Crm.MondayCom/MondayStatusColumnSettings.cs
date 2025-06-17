using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Crm.MondayCom;

public class MondayStatusColumnSettings
{
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; set; }

    [JsonPropertyName("done_colors")]
    public List<int>? DoneColors { get; set; }

    [JsonPropertyName("hide_footer")]
    public bool HideFooter { get; set; }

    [JsonPropertyName("color_mapping")]
    public Dictionary<string, int>? ColorMapping { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    public (int Index, string Label)? GetLabelByName(string? labelName, bool ignoreCase = true)
    {
        if (labelName != null)
        {
            foreach (var kvp in Labels.NullSafeEnumerable())
            {
                if (0 == string.Compare(labelName, kvp.Value, ignoreCase))
                {
                    return (int.Parse(kvp.Key), kvp.Value);
                }
            }
        }
        return null;
    }

    [JsonPropertyName("labels_positions_v2")]
    public Dictionary<string, int>? LabelsPositionsV2 { get; set; }

    [JsonPropertyName("labels_colors")]
    public Dictionary<string, LabelColor>? LabelsColors { get; set; }

    public class LabelColor
    {
        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("border")]
        public string? Border { get; set; }

        [JsonPropertyName("var_name")]
        public string? VarName { get; set; }
    }
}

