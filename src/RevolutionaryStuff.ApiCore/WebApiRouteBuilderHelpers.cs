using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

    private sealed record ProducesFileMetadata(HttpStatusCode StatusCode, string[] ContentTypes);

    extension<IB>(IB source) where IB : IEndpointConventionBuilder
    {
        public IB ManagementApi(string name)
                => source.WithTags(TagNames.Management).WithName(name);
        public IB DevelopmentApi(string name)
                => source.WithTags(TagNames.Development).WithName(name);
        internal IB WithTagODataQuery()
                => source.WithTags(TagNames.ODataQuery);
    }





    extension(RouteHandlerBuilder source)
    {
        public RouteHandlerBuilder ProducesHttpRedirect()
                => source.Produces<IResult>((int)HttpStatusCode.Redirect);
        public RouteHandlerBuilder ProducesHttpRedirectToImage()
                => source.Produces<IResult>((int)HttpStatusCode.Redirect, MimeType.Application.OctetStream.PrimaryContentType);
        public RouteHandlerBuilder ProducesExistingFile(params string[] expectedContentTypes)
                => source.ProducesFile(HttpStatusCode.OK, expectedContentTypes);
        public RouteHandlerBuilder ProducesCreatedFile(params string[] expectedContentTypes)
                => source.ProducesFile(HttpStatusCode.Created, expectedContentTypes);
        internal RouteHandlerBuilder ProducesFile(HttpStatusCode httpStatusCode, params string[] expectedContentTypes)
        {
            var metadata = new ProducesFileMetadata(httpStatusCode, expectedContentTypes);
            source.WithMetadata(metadata);
            return source.AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
        {
            if (!ReferenceEquals(metadata, context.Description.ActionDescriptor.EndpointMetadata.OfType<ProducesFileMetadata>().FirstOrDefault()))
                return Task.CompletedTask;

            operation.Responses ??= [];

            Dictionary<string, IOpenApiMediaType> successContent = new()
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
            foreach (var expectedContentType in expectedContentTypes)
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
            operation.Responses[((int)httpStatusCode).ToString()] = new OpenApiResponse
            {
                Description = "File downloaded successfully",
                Content = successContent
            };
            operation.Responses[((int)HttpStatusCode.NotFound).ToString()] = new OpenApiResponse
            {
                Description = "File not found"
            };
            return Task.CompletedTask;
        });
        }
    }
}
