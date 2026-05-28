using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Applets.Services.TextTemplateRenderers;

public interface IMustacheTextTemplateRenderer : ITextTemplateRenderer
{
    public const string ServiceName = "mustache";
}
