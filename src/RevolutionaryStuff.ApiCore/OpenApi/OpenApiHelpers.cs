using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi;

namespace RevolutionaryStuff.ApiCore.OpenApi;

public static class OpenApiHelpers
{
    private sealed record InputPayloadMetadata(string Summary, string Description, string ContentType);

    extension(RouteHandlerBuilder source)
    {
        /// <summary>
        /// Adds OpenAPI metadata describing a required binary file upload request body.
        /// </summary>
        /// <returns>The route handler builder, to support fluent configuration.</returns>
        public RouteHandlerBuilder ExpectsBinaryInputPayload()
            => ConfigureInputPayload(source,
                "Upload a binary file",
                "Uploads a binary file to the server.",
                MimeType.Application.OctetStream.PrimaryContentType);
        /// <summary>
        /// Adds OpenAPI metadata describing a required image file upload request body.
        /// </summary>
        /// <returns>The route handler builder, to support fluent configuration.</returns>
        public RouteHandlerBuilder ExpectsImageInputPayload()
            => ConfigureInputPayload(source,
                "Upload an image file",
                "Uploads an image file to the server.",
                MimeType.Image.Any.PrimaryContentType);
    }

    private static RouteHandlerBuilder ConfigureInputPayload(RouteHandlerBuilder source, string summary, string description, string contentType)
    {
        var metadata = new InputPayloadMetadata(summary, description, contentType);
        source.WithMetadata(metadata);
        return source.AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
        {
            if (!ReferenceEquals(metadata, context.Description.ActionDescriptor.EndpointMetadata.OfType<InputPayloadMetadata>().FirstOrDefault()))
                return Task.CompletedTask;

            operation.Summary = summary;
            operation.Description = description;
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, IOpenApiMediaType>
                {
                    [contentType] = new OpenApiMediaType
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
        });
    }
}
