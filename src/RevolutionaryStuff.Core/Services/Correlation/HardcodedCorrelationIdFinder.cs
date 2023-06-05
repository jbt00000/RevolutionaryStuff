namespace RevolutionaryStuff.Core.Services.Correlation;

public class HardcodedCorrelationIdFinder : ICorrelationIdFinder
{
    public string CorrelationId { get; set; }

    IList<string> ICorrelationIdFinder.CorrelationIds
        => CorrelationId == null ? Empty.StringArray : new[] { CorrelationId };
}
