using RevolutionaryStuff.Core.ApplicationParts.TextTemplates;

namespace RevolutionaryStuff.Applets.Services.TextTemplateRenderers;

public interface IScribanTextTemplateRenderer : ITextTemplateRenderer
{
    const string ServiceName = "scriban";
}
