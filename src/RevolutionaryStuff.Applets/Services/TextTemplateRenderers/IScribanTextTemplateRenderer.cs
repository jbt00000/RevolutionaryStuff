using RevolutionaryStuff.Core.ApplicationParts.TextTemplates;

namespace RevolutionaryStuff.Applets.Services.TextTemplateRenderers;

public interface IScribanTextTemplateRenderer : ITextTemplateRenderer
{
    public const string ServiceName = "scriban";
}
