using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace RevolutionaryStuff.ApiCore.OpenApi;

public static class OpenApiHelpers
{
    private sealed record InputPayloadMetadata(string Summary, string Description, string ContentType);

    /// <summary>
    /// Adds OpenAPI metadata describing a required binary file upload request body.
    /// </summary>
    /// <param name="builder">The route handler builder to configure.</param>
    /// <returns>The route handler builder, to support fluent configuration.</returns>
    public static RouteHandlerBuilder ExpectsBinaryInputPayload(this RouteHandlerBuilder builder)
        => builder.WithMetadata(new InputPayloadMetadata(
            "Upload a binary file",
            "Uploads a binary file to the server.",
            MimeType.Application.OctetStream.PrimaryContentType));

    /// <summary>
    /// Adds OpenAPI metadata describing a required image file upload request body.
    /// </summary>
    /// <param name="builder">The route handler builder to configure.</param>
    /// <returns>The route handler builder, to support fluent configuration.</returns>
    public static RouteHandlerBuilder ExpectsImageInputPayload(this RouteHandlerBuilder builder)
        => builder.WithMetadata(new InputPayloadMetadata(
            "Upload an image file",
            "Uploads an image file to the server.",
            MimeType.Image.Any.PrimaryContentType));

    internal sealed class InputPayloadOperationTransformer : IOpenApiOperationTransformer
    {
        /// <summary>
        /// Applies upload request-body metadata to the OpenAPI operation for a configured endpoint.
        /// </summary>
        /// <param name="operation">The OpenAPI operation to update.</param>
        /// <param name="context">The context for the operation being transformed.</param>
        /// <param name="cancellationToken">The token used to cancel the transformation.</param>
        /// <returns>A task representing the transformation.</returns>
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<InputPayloadMetadata>()
                .FirstOrDefault();
            if (metadata is null)
                return Task.CompletedTask;

            operation.Summary = metadata.Summary;
            operation.Description = metadata.Description;
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [metadata.ContentType] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary"
                        }
                    }
                }
            };

            return Task.CompletedTask;
        }
    }
}
