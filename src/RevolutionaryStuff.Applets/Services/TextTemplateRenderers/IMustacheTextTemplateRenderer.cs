using RevolutionaryStuff.Core.ApplicationParts.TextTemplates;

namespace RevolutionaryStuff.Applets.Services.TextTemplateRenderers;

public interface IMustacheTextTemplateRenderer : ITextTemplateRenderer
{
    public const string ServiceName = "mustache";
}
