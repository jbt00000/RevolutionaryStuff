using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.ApiCore.OpenApi;

namespace RevolutionaryStuff.ApiCore.Tests.OpenApi;

[TestClass]
public class OpenApiFileMetadataTests
{
    [TestMethod]
    public async Task FileHelpersDescribeUploadAndDownloadContracts()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddOpenApi(options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);

        await using var app = builder.Build();
        app.MapPost("/binary", () => Results.Ok()).ExpectsBinaryInputPayload();
        app.MapPost("/image", () => Results.Ok()).ExpectsImageInputPayload();
        app.MapGet("/existing", () => Results.Ok()).ProducesExistingFile("application/pdf");
        app.MapPost("/created", () => Results.Ok()).ProducesCreatedFile("image/png");
        app.MapPost("/repeated-upload", () => Results.Ok()).ExpectsBinaryInputPayload().ExpectsImageInputPayload();
        app.MapGet("/repeated-download", () => Results.Ok()).ProducesExistingFile("application/pdf").ProducesCreatedFile("image/png");
        app.MapOpenApi();
        await app.StartAsync();

        try
        {
            using var client = new HttpClient { BaseAddress = new(app.Urls.Single()) };
            using var response = await client.GetAsync("/openapi/v1.json");
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var paths = document.RootElement.GetProperty("paths");

            AssertUpload(paths.GetProperty("/binary").GetProperty("post"), "application/octet-stream", "Upload a binary file", "Uploads a binary file to the server.");
            AssertUpload(paths.GetProperty("/image").GetProperty("post"), "image/*", "Upload an image file", "Uploads an image file to the server.");
            AssertDownload(paths.GetProperty("/existing").GetProperty("get"), "200", "application/pdf");
            AssertDownload(paths.GetProperty("/created").GetProperty("post"), "201", "image/png");
            AssertUpload(paths.GetProperty("/repeated-upload").GetProperty("post"), "application/octet-stream", "Upload a binary file", "Uploads a binary file to the server.");
            var repeatedDownload = paths.GetProperty("/repeated-download").GetProperty("get");
            AssertDownload(repeatedDownload, "200", "application/pdf");
            Assert.IsFalse(repeatedDownload.GetProperty("responses").TryGetProperty("201", out _));
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static void AssertUpload(JsonElement operation, string contentType, string summary, string description)
    {
        Assert.AreEqual(summary, operation.GetProperty("summary").GetString());
        Assert.AreEqual(description, operation.GetProperty("description").GetString());
        var body = operation.GetProperty("requestBody");
        Assert.IsTrue(body.GetProperty("required").GetBoolean());
        AssertBinarySchema(body.GetProperty("content").GetProperty(contentType));
    }

    private static void AssertDownload(JsonElement operation, string statusCode, string contentType)
    {
        var responses = operation.GetProperty("responses");
        var success = responses.GetProperty(statusCode);
        Assert.AreEqual("File downloaded successfully", success.GetProperty("description").GetString());
        var content = success.GetProperty("content");
        AssertBinarySchema(content.GetProperty("application/octet-stream"));
        AssertBinarySchema(content.GetProperty(contentType));
        Assert.AreEqual("File not found", responses.GetProperty("404").GetProperty("description").GetString());
    }

    private static void AssertBinarySchema(JsonElement mediaType)
    {
        var schema = mediaType.GetProperty("schema");
        Assert.AreEqual("string", schema.GetProperty("type").GetString());
        Assert.AreEqual("binary", schema.GetProperty("format").GetString());
    }
}
