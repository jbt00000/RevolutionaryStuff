using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core.ApplicationParts;

public class BuilderConfig
{
    public const string ConfigSectionName = "BuilderConfig";

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; }

    [JsonPropertyName("builtAt")]
    public DateTimeOffset BuiltAt { get; set; } = DateTime.MinValue;

    [JsonPropertyName("builtBy")]
    public string BuiltBy { get; set; } = "Set this in the appsettings.builder.json file";

    [JsonPropertyName("pipelineName")]
    public string PipelineName { get; set; } = "none";

    [JsonPropertyName("triggerName")]
    public string TriggerName { get; set; } = "manual";

    [JsonPropertyName("commitHash")]
    public string CommitHash { get; set; } = "00";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.0";
}
