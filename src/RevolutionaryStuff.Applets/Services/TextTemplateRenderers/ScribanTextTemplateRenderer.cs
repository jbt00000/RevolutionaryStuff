using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using Scriban;
using Scriban.Runtime;
using System.Text.Json;

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

    private static object ConvertTemplateData(object templateData)
    {
        return templateData switch
        {
            // If it's already a dictionary, use it directly
            IDictionary<string, object> => templateData,

            // If it's a JsonElement, convert to Dictionary
            JsonElement jsonElement => ConvertJsonElementToDictionary(jsonElement),

            // If it's a JsonDocument, get the root element
            JsonDocument jsonDoc => ConvertJsonElementToDictionary(jsonDoc.RootElement),

            // For other objects, use as-is (Scriban handles reflection)
            _ => templateData
        };
    }

    private static Dictionary<string, object> ConvertJsonElementToDictionary(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("JsonElement must be an object to be used as template data", nameof(element));
        }

        var dictionary = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = ConvertJsonValue(property.Value);
        }

        return dictionary;
    }

    private static object ConvertJsonValue(JsonElement element)
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
