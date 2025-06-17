using System.Text.Json.Serialization;

public class MondayGroup
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    [JsonPropertyName("color ")]
    public string? Color { get; set; }

    public override string ToString()
        => $"[{Title}] - {Id}";
}
