namespace RevolutionaryStuff.Dapr.Configs;
internal class DaprConfig
{
    public const string ConfigSectionName = "dapr";

    public string? MyStateStoreName { get; set; }
    public string? SharedStateStoreName { get; set; }
}
