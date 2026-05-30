using RevolutionaryStuff.Core.ApplicationParts.TextTemplates;

namespace RevolutionaryStuff.AspNetCore.Services;

public interface IRazorTextTemplateRenderer : ITextTemplateRenderer
{
    public const string ServiceName = "razor";
}
