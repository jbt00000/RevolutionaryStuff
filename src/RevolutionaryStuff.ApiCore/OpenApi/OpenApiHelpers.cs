using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

namespace RevolutionaryStuff.ApiCore.OpenApi;

public static class OpenApiHelpers
{
    public static RouteHandlerBuilder ExpectsBinaryInputPayload(this RouteHandlerBuilder builder)
        => builder.WithOpenApi(operation => new(operation)
        {
            Summary = "Upload a binary file",
            Description = "Uploads a binary file to the server.",
            RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content =
                {
                    [MimeType.Application.OctetStream.PrimaryContentType] = new OpenApiMediaType
                    {
                        Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    }
                }
            }
        });

    public static RouteHandlerBuilder ExpectsImageInputPayload(this RouteHandlerBuilder builder)
        => builder.WithOpenApi(operation => new(operation)
        {
            Summary = "Upload an image file",
            Description = "Uploads an image file to the server.",
            RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content =
                {
                    [MimeType.Image.Any.PrimaryContentType] = new OpenApiMediaType
                    {
                        Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    }
                }
            }
        });
}
