using System.Threading;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace RevolutionaryStuff.ApiCore.OpenApi;
internal class OpenApiOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken = default)
    {
        foreach (var op in context.Description.ActionDescriptor.EndpointMetadata.OfType<OpenApiOperation>())
        {
            operation.Summary = op.Summary ?? operation.Summary;
            operation.Description = op.Description ?? operation.Description;
            operation.RequestBody = op.RequestBody ?? operation.RequestBody;

        }
        return Task.CompletedTask;
    }

    public Task FinalizeTransformer() => Task.CompletedTask;
}
