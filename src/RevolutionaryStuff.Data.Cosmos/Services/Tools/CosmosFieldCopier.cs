using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Data.Cosmos.Services.Tools;

/// <summary>
/// Cosmos container runner that copies the value of <see cref="CosmosFieldCopierConfig.SourceFieldName"/> into
/// <see cref="CosmosFieldCopierConfig.DestFieldName"/> for every document that contains the source field but not
/// the destination field.
///
/// The operation is idempotent: documents that already have the destination field are skipped.
/// </summary>
internal class CosmosFieldCopier : RevolutionaryStuff.Core.RevolutionaryStuffService, ICosmosFieldCopier
{
    public CosmosFieldCopier(RevolutionaryStuffServiceConstrutorArgs constructorArgs)
        : base(constructorArgs)
    {
    }

    #region ICosmosFieldCopier

    async Task<CosmosFieldCopierResult> ICosmosContainerRunner<CosmosFieldCopierConfig, CosmosFieldCopierResult>.RunAsync(
        Container container, CosmosFieldCopierConfig config, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(config);
        Requires.Text(config.SourceFieldName, nameof(config.SourceFieldName));
        Requires.Text(config.DestFieldName, nameof(config.DestFieldName));

        int scanned = 0, modified = 0, skipped = 0;
        var errors = new List<string>();
        var props = (await container.ReadContainerAsync(cancellationToken: ct)).Resource;
        var pkPaths = props.PartitionKeyPaths?.ToList() ?? [];

        var iterator = container.GetItemQueryStreamIterator(
            "SELECT * FROM c",
            requestOptions: new QueryRequestOptions { MaxItemCount = config.PageSize });

        while (iterator.HasMoreResults)
        {
            using var pageResponse = await iterator.ReadNextAsync(ct);

            if (!pageResponse.IsSuccessStatusCode)
            {
                var msg = $"Feed read error on container {container.Id}: {pageResponse.StatusCode}";
                Logger.LogWarning(msg);
                errors.Add(msg);
                continue;
            }

            using var feedDoc = await JsonDocument.ParseAsync(pageResponse.Content, cancellationToken: ct);
            if (!feedDoc.RootElement.TryGetProperty("Documents", out var documents))
                continue;

            foreach (var doc in documents.EnumerateArray())
            {
                scanned++;
                var id = doc.TryGetProperty("id", out var idEl) ? idEl.GetString() : "(unknown)";

                try
                {
                    // Idempotency guard — dest field already present means nothing to do.
                    if (doc.TryGetProperty(config.DestFieldName, out _))
                    {
                        skipped++;
                        continue;
                    }

                    // Source field must be present.
                    if (!doc.TryGetProperty(config.SourceFieldName, out var sourceValue))
                    {
                        skipped++;
                        continue;
                    }

                    var pk = BuildPartitionKey(doc, pkPaths);
                    var patchValue = ExtractPatchValue(sourceValue);

                    using var patchResp = await container.PatchItemStreamAsync(
                        id!,
                        pk,
                        [PatchOperation.Add<object?>($"/{config.DestFieldName}", patchValue)],
                        cancellationToken: ct);

                    if (patchResp.IsSuccessStatusCode)
                    {
                        modified++;
                        Logger.LogDebug("Copied field in {Id} in {Container}", id, container.Id);
                    }
                    else
                    {
                        var msg = $"{container.Id}/{id}: patch failed with {patchResp.StatusCode}";
                        errors.Add(msg);
                        Logger.LogWarning(msg);
                    }
                }
                catch (Exception ex)
                {
                    var msg = $"{container.Id}/{id}: {ex.Message}";
                    errors.Add(msg);
                    Logger.LogError(ex, "Error processing {Id} in {Container}", id, container.Id);
                }
            }
        }

        Logger.LogInformation(
            "Container {Container}: scanned={Scanned}, modified={Modified}, skipped={Skipped}, errors={Errors}",
            container.Id, scanned, modified, skipped, errors.Count);

        return new CosmosFieldCopierResult(scanned, modified, skipped, errors.AsReadOnly());
    }

    #endregion

    #region JSON helpers

    /// <summary>
    /// Extracts a patch-safe .NET value from a <see cref="JsonElement"/> so that
    /// <see cref="PatchOperation.Add{T}"/> serializes it correctly regardless of
    /// the underlying Cosmos serializer.
    /// </summary>
    private static object? ExtractPatchValue(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.String => el.GetString(),
        JsonValueKind.Number when el.TryGetInt64(out var l) => l,
        JsonValueKind.Number => el.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => JsonNode.Parse(el.GetRawText())
    };

    #endregion

    #region Partition key helpers

    private static PartitionKey BuildPartitionKey(JsonElement doc, List<string> pkPaths)
    {
        if (pkPaths.Count == 0)
            return PartitionKey.None;

        if (pkPaths.Count == 1)
            return new PartitionKey(GetPathValue(doc, pkPaths[0]));

        var builder = new PartitionKeyBuilder();
        foreach (var path in pkPaths)
            builder.Add(GetPathValue(doc, path));
        return builder.Build();
    }

    private static string? GetPathValue(JsonElement el, string path)
    {
        var current = el;
        foreach (var segment in path.TrimStart('/').Split('/'))
        {
            if (!current.TryGetProperty(segment, out current))
                return null;
        }
        return current.ValueKind == JsonValueKind.String
            ? current.GetString()
            : current.GetRawText().Trim('"');
    }

    #endregion
}
