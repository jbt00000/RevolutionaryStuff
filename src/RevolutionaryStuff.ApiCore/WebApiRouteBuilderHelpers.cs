using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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
        => builder.WithOpenApi(operation =>
        {
            Dictionary<string, Microsoft.OpenApi.Models.OpenApiMediaType> successContent = new()
            {
                [MimeType.Application.OctetStream.PrimaryContentType] = new Microsoft.OpenApi.Models.OpenApiMediaType()
                {
                    Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                }
            };
            foreach (var expectedContentType in expectedContentTypes)
            {
                successContent[expectedContentType] = new Microsoft.OpenApi.Models.OpenApiMediaType()
                {
                    Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
            }
            operation.Responses[((int)httpStatusCode).ToString()] = new Microsoft.OpenApi.Models.OpenApiResponse
            {
                Description = "File downloaded successfully",
                Content = successContent,
            };
            operation.Responses[((int)HttpStatusCode.NotFound).ToString()] = new Microsoft.OpenApi.Models.OpenApiResponse
            {
                Description = "File not found"
            };
            return operation;
        });
}
