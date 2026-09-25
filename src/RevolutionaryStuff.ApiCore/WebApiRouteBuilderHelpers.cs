using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace RevolutionaryStuff.ApiCore;

public static class WebApiRouteBuilderHelpers
{
    public static class TagNames
    {
        public const string Development = "Development";
        public const string Management = "Management";
        public const string ODataQuery = "ODataQuery";
    }
    public static IB ManagementApi<IB>(this IB b, string name)
        where IB : IEndpointConventionBuilder
        => b.WithTags(TagNames.Management).WithName(name);

    public static IB DevelopmentApi<IB>(this IB b, string name)
        where IB : IEndpointConventionBuilder
        => b.WithTags(TagNames.Development).WithName(name);

    internal static IB WithTagODataQuery<IB>(this IB b)
        where IB : IEndpointConventionBuilder
        => b.WithTags(TagNames.ODataQuery);

    public static RouteHandlerBuilder ProducesHttpRedirect(this RouteHandlerBuilder builder)
        => builder.Produces<IResult>((int)HttpStatusCode.Redirect);
    public static RouteHandlerBuilder ProducesHttpRedirectToImage(this RouteHandlerBuilder builder)
        => builder.Produces<IResult>((int)HttpStatusCode.Redirect, MimeType.Application.OctetStream.PrimaryContentType);
    public static RouteHandlerBuilder ProducesExistingFile(this RouteHandlerBuilder builder, params string[] expectedContentTypes)
        => builder.ProducesFile(HttpStatusCode.OK, expectedContentTypes);
    public static RouteHandlerBuilder ProducesCreatedFile(this RouteHandlerBuilder builder, params string[] expectedContentTypes)
        => builder.ProducesFile(HttpStatusCode.Created, expectedContentTypes);
    internal static RouteHandlerBuilder ProducesFile(this RouteHandlerBuilder builder, HttpStatusCode httpStatusCode, params string[] expectedContentTypes)
        => builder.WithMetadata(new ProducesFileMetadata(httpStatusCode, expectedContentTypes));

    private sealed record ProducesFileMetadata(HttpStatusCode StatusCode, string[] ContentTypes);

    internal sealed class ProducesFileOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<ProducesFileMetadata>()
                .FirstOrDefault();
            if (metadata is null)
                return Task.CompletedTask;

            operation.Responses ??= [];

            Dictionary<string, OpenApiMediaType> successContent = new()
            {
                [MimeType.Application.OctetStream.PrimaryContentType] = new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "binary"
                    }
                }
            };
            foreach (var expectedContentType in metadata.ContentTypes)
            {
                successContent[expectedContentType] = new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "binary"
                    }
                };
            }
            operation.Responses[((int)metadata.StatusCode).ToString()] = new OpenApiResponse
            {
                Description = "File downloaded successfully",
                Content = successContent
            };
            operation.Responses[((int)HttpStatusCode.NotFound).ToString()] = new OpenApiResponse
            {
                Description = "File not found"
            };
            return Task.CompletedTask;
        }
    }
}
