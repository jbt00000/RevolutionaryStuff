namespace RevolutionaryStuff.Data.Cosmos.Services.Setup;

public class ContainerSetupInfo
{ 
    public required string ContainerId { get; set; }
    public required string PartitionKeyPath { get; set; }
    public IList<string>? UniqueKeyPaths { get; set; }
    public IList<StoredProcedureBootstrapInfo>? StoredProcedureInfos { get; set; }
    public bool DeleteExistingTriggers { get; set; }
    public IList<TriggerBootstrapInfo>? TriggerInfos { get; set; }
}
