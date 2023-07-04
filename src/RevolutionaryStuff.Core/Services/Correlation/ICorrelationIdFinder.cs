namespace RevolutionaryStuff.Core.Services.Correlation;

/// <summary>
/// Finds a correlation Id
/// </summary>
public interface ICorrelationIdFinder
{
    /// <summary>
    /// Returns the correlation Id if one from this source was found, else null
    /// </summary>
    IList<string> CorrelationIds { get; }
}
