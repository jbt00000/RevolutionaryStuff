using System.Text.Json;
using RevolutionaryStuff.Core.ApplicationParts.TextTemplates;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using Scriban;
using Scriban.Runtime;

namespace RevolutionaryStuff.Applets.Services.TextTemplateRenderers;

[NamedService(IScribanTextTemplateRenderer.ServiceName)]
internal class ScribanTextTemplateRenderer : IScribanTextTemplateRenderer
{
    Task<string> ITextTemplateRenderer.RenderAsync(string templateText, object templateData, RenderOptions? options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateText);
        ArgumentNullException.ThrowIfNull(templateData);

        // Parse the template
        var template = Template.Parse(templateText);

        if (template.HasErrors)
        {
            var errors = string.Join(", ", template.Messages.Select(m => m.Message));
            throw new ArgumentException($"Template parsing failed: {errors}", nameof(templateText));
        }

        // Convert templateData to a format Scriban can use
        var dataForRendering = ConvertTemplateData(templateData);

        // Create a template context with the data
        var context = new TemplateContext();
        var scriptObject = new ScriptObject();

        if (dataForRendering is IDictionary<string, object> dict)
        {
            scriptObject.Import(dict);
        }
        else
        {
            scriptObject.Import(dataForRendering);
        }

        context.PushGlobal(scriptObject);

        // Render the template
        var result = template.Render(context);

        return Task.FromResult(result);
    }

    private static readonly JsonHelpers.ToPocoSettings ScribanUnJsonElementSettings = new()
    {
        DictionaryComparer = StringComparer.OrdinalIgnoreCase
    };

    private static object ConvertTemplateData(object templateData)
    {
        return templateData switch
        {
            // If it's already a dictionary, use it directly
            IDictionary<string, object> => templateData,

            // If it's a JsonElement, convert to Dictionary
            JsonElement jsonElement => JsonHelpers.ToPoco(jsonElement, ScribanUnJsonElementSettings),

            // If it's a JsonDocument, get the root element
            JsonDocument jsonDoc => JsonHelpers.ToPoco(jsonDoc.RootElement, ScribanUnJsonElementSettings),

            // For other objects, use as-is (Scriban handles reflection)
            _ => templateData
        };
    }
}
