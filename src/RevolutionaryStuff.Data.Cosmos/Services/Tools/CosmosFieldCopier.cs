using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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

                    var transformed = CopyField(doc, config.SourceFieldName, config.DestFieldName, sourceValue);
                    var pk = BuildPartitionKey(doc, pkPaths);

                    using var payload = new MemoryStream(transformed);
                    using var upsertResp = await container.UpsertItemStreamAsync(payload, pk, cancellationToken: ct);

                    if (upsertResp.IsSuccessStatusCode)
                    {
                        modified++;
                        Logger.LogDebug("Copied field in {Id} in {Container}", id, container.Id);
                    }
                    else
                    {
                        var msg = $"{container.Id}/{id}: upsert failed with {upsertResp.StatusCode}";
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
    /// Returns a new JSON byte array based on <paramref name="root"/> with
    /// <paramref name="destFieldName"/> injected immediately after <paramref name="sourceFieldName"/>
    /// at the top level, carrying the same value.
    /// </summary>
    private static byte[] CopyField(JsonElement root, string sourceFieldName, string destFieldName, JsonElement sourceValue)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);

        writer.WriteStartObject();
        foreach (var prop in root.EnumerateObject())
        {
            writer.WritePropertyName(prop.Name);
            prop.Value.WriteTo(writer);

            if (prop.Name == sourceFieldName)
            {
                writer.WritePropertyName(destFieldName);
                sourceValue.WriteTo(writer);
            }
        }
        writer.WriteEndObject();
        writer.Flush();

        return ms.ToArray();
    }

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
