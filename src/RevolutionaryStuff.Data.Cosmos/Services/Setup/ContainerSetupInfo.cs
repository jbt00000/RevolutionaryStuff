using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.Cosmos.Services.Setup;

public class ContainerSetupInfo : IValidate
{
    public required string ContainerId { get; set; }
    public required List<string> PartitionKeyPaths { get; set; }
    public IList<string>? UniqueKeyPaths { get; set; }
    public IList<StoredProcedureBootstrapInfo>? StoredProcedureInfos { get; set; }
    public bool DeleteExistingTriggers { get; set; }
    public IList<TriggerBootstrapInfo>? TriggerInfos { get; set; }
    public bool EnableChangeFeed { get; set; }
    public string? LeasesContainerId { get; set; }

    public void Validate()
        => ExceptionHelpers.AggregateExceptionsAndReThrow(
            () => Requires.Text(ContainerId),
            () => Requires.True(PartitionKeyPaths.NullSafeAny())
        );

}
