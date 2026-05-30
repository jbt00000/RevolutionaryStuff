using RevolutionaryStuff.Core.ApplicationParts.TextTemplates;

namespace RevolutionaryStuff.AspNetCore.Services;

public interface IRazorTextTemplateRenderer : ITextTemplateRenderer
{
    const string ServiceName = "razor";
}
