using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using Stubble.Core.Builders;
using Stubble.Core.Interfaces;
using System.Text.Json;

namespace RevolutionaryStuff.Applets.Services.TextTemplateRenderers;

[NamedService(IMustacheTextTemplateRenderer.ServiceName)]
internal class MustacheTextTemplateRenderer : IMustacheTextTemplateRenderer
{
    private readonly IStubbleRenderer _renderer;

    public MustacheTextTemplateRenderer()
    {
        _renderer = new StubbleBuilder()
            .Configure(settings =>
            {
                settings.SetIgnoreCaseOnKeyLookup(true);
                settings.SetMaxRecursionDepth(256);
            })
            .Build();
    }

    Task<string> ITextTemplateRenderer.RenderAsync(string templateText, object templateData, RenderOptions? options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateText);
        ArgumentNullException.ThrowIfNull(templateData);

        // Convert templateData to a format Stubble can use
        var dataForRendering = ConvertTemplateData(templateData);

        // Render the template
        var result = _renderer.Render(templateText, dataForRendering);
        return Task.FromResult(result);
    }

    private static object ConvertTemplateData(object templateData)
    {
        return templateData switch
        {
            // If it's already a dictionary or anonymous object, use it directly
            IDictionary<string, object> => templateData,

            // If it's a JsonElement, convert to Dictionary
            JsonElement jsonElement => ConvertJsonElementToDictionary(jsonElement),

            // If it's a JsonDocument, get the root element
            JsonDocument jsonDoc => ConvertJsonElementToDictionary(jsonDoc.RootElement),

            // For dynamic or other objects, use as-is (Stubble handles reflection)
            _ => templateData
        };
    }

    private static Dictionary<string, object?> ConvertJsonElementToDictionary(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("JsonElement must be an object to be used as template data", nameof(element));
        }

        var dictionary = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = ConvertJsonValue(property.Value);
        }

        return dictionary;
    }

    private static object? ConvertJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonElementToDictionary(element),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonValue).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }
}
