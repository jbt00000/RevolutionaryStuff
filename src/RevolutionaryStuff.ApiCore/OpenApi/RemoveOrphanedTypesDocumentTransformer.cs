using System.Threading;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace RevolutionaryStuff.ApiCore.OpenApi;

public sealed class RemoveOrphanedTypesDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var referencedSchemas = new HashSet<string>();

        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                CollectReferencedSchemas(operation.RequestBody?.Content, referencedSchemas);
                foreach (var response in operation.Responses.Values)
                {
                    CollectReferencedSchemas(response.Content, referencedSchemas);
                }
                foreach (var parameter in operation.Parameters.NullSafeEnumerable())
                {
                    CollectReferencedSchemas(parameter.Schema, referencedSchemas);
                }
            }
        }

        for (; ; )
        {
            var added = 0;
            foreach (var rs in referencedSchemas.ToArray())
            {
                var s = document.Components.Schemas.GetValue(rs);
                if (s != null)
                {
                    var before = referencedSchemas.Count;
                    CollectReferencedSchemas(s, referencedSchemas);
                    added += referencedSchemas.Count - before;
                }
            }
            if (added == 0) break;
        }

        var orphanedSchemas = document.Components.Schemas.Keys
            .Where(schemaName => !referencedSchemas.Contains(schemaName))
            .ToList();

        System.Diagnostics.Trace.WriteLine(
$"""
REFERENCED:
{referencedSchemas.Order().Format("\n")}

ORPHANED:
{orphanedSchemas.Order().Format("\n")}
""");

        foreach (var orphanedSchema in orphanedSchemas)
        {
            //               document.Components.Schemas.Remove(orphanedSchema);
        }
        return Task.CompletedTask;
    }

    private void CollectReferencedSchemas(IDictionary<string, OpenApiMediaType> content, HashSet<string> referencedSchemas)
    {
        if (content == null) return;

        foreach (var mediaType in content.Values)
        {
            if (mediaType.Schema is OpenApiSchemaReference mediaSchemaRef)
            {
                referencedSchemas.Add(mediaSchemaRef.Reference.Id);
                if (mediaSchemaRef.Reference.Id == "CreateSurveyResponseResult")
                {
                    Stuff.NoOp(content);
                }
                CollectReferencedSchemas(mediaSchemaRef, referencedSchemas);
            }
            else if (mediaType.Schema?.Items != null)
            {
                CollectReferencedSchemas(mediaType.Schema.Items, referencedSchemas);
            }
        }
    }

    private void CollectReferencedSchemas(IOpenApiSchema schema, HashSet<string> referencedSchemas, int depth = 0)
    {
        if (schema == null || depth > 10) return;

        if (schema is OpenApiSchemaReference schemaRef)
        {
            referencedSchemas.Add(schemaRef.Reference.Id);
        }

        if (schema.Items != null)
        {
            CollectReferencedSchemas(schema.Items, referencedSchemas, depth + 1);
        }

        if (schema.Properties != null)
        {
            foreach (var property in schema.Properties.Values)
                CollectReferencedSchemas(property, referencedSchemas, depth + 1);
        }

        if (schema.AdditionalProperties != null) CollectReferencedSchemas(schema.AdditionalProperties, referencedSchemas, depth + 1);

        if (schema.AllOf != null)
        {
            foreach (var item in schema.AllOf) CollectReferencedSchemas(item, referencedSchemas, depth + 1);
        }

        if (schema.AnyOf != null)
        {
            foreach (var item in schema.AnyOf) CollectReferencedSchemas(item, referencedSchemas, depth + 1);
        }

        if (schema.OneOf != null)
        {
            foreach (var item in schema.OneOf) CollectReferencedSchemas(item, referencedSchemas, depth + 1);
        }
    }
}
