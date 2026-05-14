using System.Collections.Generic;

namespace RevolutionaryStuff.Data.Cosmos.Services.Tools;

/// <summary>
/// Per-call settings for <see cref="ICosmosFieldCopier"/>.
/// </summary>
public class CosmosFieldCopierConfig
{
    /// <summary>The JSON property name whose value will be read.</summary>
    public string SourceFieldName { get; set; } = string.Empty;

    /// <summary>The JSON property name that will be written with the source value.</summary>
    public string DestFieldName { get; set; } = string.Empty;

    /// <summary>How many documents to fetch per feed page.</summary>
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Result produced by <see cref="ICosmosFieldCopier"/> for a single container pass.
/// </summary>
public record CosmosFieldCopierResult(
    int DocumentsScanned,
    int DocumentsModified,
    int DocumentsSkipped,
    IReadOnlyList<string> Errors);

/// <summary>
/// Copies the value of a source field into a destination field for every document in a
/// Cosmos container that contains the source field but not yet the destination field.
/// </summary>
public interface ICosmosFieldCopier : ICosmosContainerRunner<CosmosFieldCopierConfig, CosmosFieldCopierResult>
{
}
