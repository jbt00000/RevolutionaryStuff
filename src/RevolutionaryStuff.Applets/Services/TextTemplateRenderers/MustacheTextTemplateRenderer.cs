using System.Text.Json;
using RevolutionaryStuff.Core.ApplicationParts.TextTemplates;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using Stubble.Core.Builders;
using Stubble.Core.Interfaces;

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

    private static readonly JsonHelpers.ToPocoSettings MustacheUnJsonElementSettings = new()
    {
        DictionaryComparer = StringComparer.OrdinalIgnoreCase
    };

    private static object ConvertTemplateData(object templateData)
    {
        return templateData switch
        {
            // If it's already a dictionary or anonymous object, use it directly
            IDictionary<string, object> => templateData,

            // If it's a JsonElement, convert to Dictionary
            JsonElement jsonElement => JsonHelpers.ToPoco(jsonElement, MustacheUnJsonElementSettings),

            // If it's a JsonDocument, get the root element
            JsonDocument jsonDoc => JsonHelpers.ToPoco(jsonDoc.RootElement, MustacheUnJsonElementSettings),

            // For dynamic or other objects, use as-is (Stubble handles reflection)
            _ => templateData
        };
    }
}
