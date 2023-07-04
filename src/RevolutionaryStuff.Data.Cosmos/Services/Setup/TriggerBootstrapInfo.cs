namespace RevolutionaryStuff.Data.Cosmos.Services.Setup;

public class TriggerBootstrapInfo
{
    public required string TriggerBaseName { get; set; }
    public required string TriggerText { get; set; }
    public required IList<TriggerTypeEnum> TriggerTypes { get; set; }
    public required IList<TriggerOperationEnum> TriggerOperations { get; set; }
    public required string TriggerIdFormat { get; set; } = "{0}_{1}_{2}";
}
