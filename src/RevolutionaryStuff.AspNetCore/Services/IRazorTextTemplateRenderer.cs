using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.AspNetCore.Services;

public interface IRazorTextTemplateRenderer : ITextTemplateRenderer
{
    public const string ServiceName = "razor";
}
