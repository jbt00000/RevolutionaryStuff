using RevolutionaryStuff.Core.ApplicationParts.TextTemplates;

namespace RevolutionaryStuff.Applets.Services.TextTemplateRenderers;

public interface IMustacheTextTemplateRenderer : ITextTemplateRenderer
{
    const string ServiceName = "mustache";
}
