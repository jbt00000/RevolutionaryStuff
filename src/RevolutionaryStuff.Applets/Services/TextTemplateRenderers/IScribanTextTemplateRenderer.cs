using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Applets.Services.TextTemplateRenderers;

public interface IScribanTextTemplateRenderer : ITextTemplateRenderer
{
    public const string ServiceName = "scriban";
}
