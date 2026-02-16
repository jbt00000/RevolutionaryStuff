using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi;

namespace RevolutionaryStuff.ApiCore.OpenApi;

public static class OpenApiHelpers
{
    public static RouteHandlerBuilder ExpectsBinaryInputPayload(this RouteHandlerBuilder builder)
        => builder.WithOpenApi(operation => new(operation ?? new OpenApiOperation())
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
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary"
                        }
                    }
                }
            }
        });

    public static RouteHandlerBuilder ExpectsImageInputPayload(this RouteHandlerBuilder builder)
        => builder.WithOpenApi(operation => new(operation ?? new OpenApiOperation())
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
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary"
                        }
                    }
                }
            }
        });
}
