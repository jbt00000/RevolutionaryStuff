using System.Threading;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace RevolutionaryStuff.ApiCore.OpenApi;

public sealed class RemoveNullEnumValuesDocumentTransformer
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (document.Components?.Schemas is null)
            return Task.CompletedTask;

        foreach (var schema in document.Components.Schemas.Values)
        {
            if (schema is not OpenApiSchema concreteSchema ||
                concreteSchema.Enum is null)
            {
                continue;
            }

            for (var i = concreteSchema.Enum.Count - 1; i >= 0; i--)
            {
                if (concreteSchema.Enum[i] is null)
                    concreteSchema.Enum.RemoveAt(i);
            }
        }

        return Task.CompletedTask;
    }
}
