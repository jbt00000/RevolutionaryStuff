using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.ApiCore.Tests.OpenApi;

public enum OpenApiEnumSchemaTestEnum
{
    [JsonStringEnumMemberName("custom-value")]
    CustomValue
}

public class OpenApiEnumSchemaTestModel
{
    /// <summary>Current state of the item.</summary>
    public OpenApiEnumSchemaTestEnum Status { get; set; }
    public OpenApiEnumSchemaTestEnum? OptionalStatus { get; set; }
    public OpenApiEnumSchemaTestEnum[] Statuses { get; set; } = [];
}

[TestClass]
public class OpenApiEnumSchemaTests
{
    /// <summary>Returns the enum schema test model.</summary>
    public static OpenApiEnumSchemaTestModel GetEnumModel()
        => new();

    [TestMethod]
    public async Task OpenApi31_UsesCustomEnumNamesForScalarsNullableValuesAndCollections()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddOpenApi(options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);
        builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        await using var app = builder.Build();
        app.MapGet("/enum", GetEnumModel).WithName("GetEnum");
        app.MapOpenApi();
        await app.StartAsync();

        try
        {
            using var client = new HttpClient { BaseAddress = new(app.Urls.Single()) };
            using var response = await client.GetAsync("/openapi/v1.json");
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            Assert.AreEqual("3.1.1", document.RootElement.GetProperty("openapi").GetString());
            var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
            var responseSchema = ResolveSchema(document.RootElement.GetProperty("paths").GetProperty("/enum").GetProperty("get")
                .GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema"), schemas);
            Assert.AreEqual("Returns the enum schema test model.", document.RootElement.GetProperty("paths").GetProperty("/enum").GetProperty("get").GetProperty("summary").GetString());
            var properties = responseSchema.GetProperty("properties");
            Assert.AreEqual("Current state of the item.", properties.GetProperty("status").GetProperty("description").GetString());

            AssertEnumValues(ResolveSchema(properties.GetProperty("status"), schemas));
            var optionalSchema = properties.GetProperty("optionalStatus");
            var optionalEnum = optionalSchema.TryGetProperty("oneOf", out var oneOf)
                ? oneOf.EnumerateArray().Select(s => ResolveSchema(s, schemas)).First(IsEnumSchema)
                : ResolveSchema(optionalSchema, schemas);
            AssertEnumValues(optionalEnum);
            var items = ResolveSchema(properties.GetProperty("statuses").GetProperty("items"), schemas);
            AssertEnumValues(items);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static JsonElement ResolveSchema(JsonElement schema, JsonElement schemas)
    {
        if (!schema.TryGetProperty("$ref", out var reference))
            return schema;

        var id = reference.GetString()!.Split('/').Last();
        return schemas.GetProperty(id);
    }

    private static bool IsEnumSchema(JsonElement schema)
        => schema.TryGetProperty("enum", out _);

    private static void AssertEnumValues(JsonElement schema)
    {
        Assert.IsTrue(schema.TryGetProperty("enum", out var enumValues), schema.GetRawText());
        var values = enumValues.EnumerateArray().Select(value => value.GetString()).ToArray();
        CollectionAssert.AreEqual(new[] { "custom-value" }, values);
    }
}
