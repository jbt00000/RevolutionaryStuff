namespace RevolutionaryStuff.Core.ApplicationParts;

public interface ITextTemplateRenderer
{
    Task<string> RenderAsync(string templateText, object templateData, RenderOptions? options = null);
}

public class RenderOptions;
