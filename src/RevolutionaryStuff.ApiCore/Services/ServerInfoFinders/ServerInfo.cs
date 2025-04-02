using System.Text.Json.Serialization;

namespace RevolutionaryStuff.ApiCore.Services.ServerInfoFinders;

public record ServerInfo
{
    [JsonPropertyName("applicationStartedAt")]
    public DateTimeOffset ApplicationStartedAt { get; init; }

    [JsonPropertyName("applicationInstanceId")]
    public Guid ApplicationInstanceId { get; init; }

    [JsonPropertyName("machineName")]
    public required string MachineName { get; init; }

    [JsonPropertyName("serverTime")]
    public DateTimeOffset ServerTime { get; init; }

    [JsonPropertyName("os")]
    public required OsInfo OperatingSystemVersion { get; init; }

    [JsonPropertyName("is64BitOperatingSystem")]
    public bool Is64BitOperatingSystem { get; set; }

    [JsonPropertyName("is64BitProcess")]
    public bool Is64BitProcess { get; set; }

    [JsonPropertyName("entryPoint")]
    public string? EntryPointAssembly { get; set; }

    [JsonPropertyName("applicationName")]
    public string? ApplicationName { get; set; }

    [JsonPropertyName("environmentName")]
    public string? EnvironmentName { get; set; }

    public class OsInfo
    {
        [JsonPropertyName("platform")]
        public PlatformID Platform { get; init; }

        [JsonPropertyName("servicePack")]
        public string? ServicePack { get; init; }

        [JsonPropertyName("version")]
        public Version Version { get; init; }

        [JsonPropertyName("versionString")]
        public string? VersionString { get; init; }

        public OsInfo(OperatingSystem os)
        {
            Platform = os.Platform;
            ServicePack = os.ServicePack.TrimOrNull();
            Version = os.Version;
            VersionString = os.VersionString.TrimOrNull();
        }
    }

    [JsonPropertyName("environmentVariables")]
    public IDictionary<string, string?>? EnvironmentVariables { get; init; }

    [JsonPropertyName("configs")]
    public IDictionary<string, string?>? Configs { get; init; }

    [JsonPropertyName("targetFramework")]
    public string? TargetFramework { get; init; }
}
