namespace RevolutionaryStuff.Core.ApplicationParts.TextTemplates;

public interface ITextTemplateRenderer
{
    Task<string> RenderAsync(string templateText, object templateData, RenderOptions? options = null);
}

//public abstract class TextTemplateRendererBase(TextTemplateRendererBaseConstructorArgs constructorArgs) 
//    : RevolutionaryStuffService(constructorArgs.BaseConstructorArgs), ITextTemplateRenderer
//{
//    Task<string> ITextTemplateRenderer.RenderAsync(string templateText, object templateData, RenderOptions? options)
//    {
//        if (templateText == null) return null;
//        return OnRenderAsync(templateText, templateData, options);
//    }

//    protected abstract Task<string> OnRenderAsync(string templateText, object templateData, RenderOptions? options);
//}

//public sealed record TextTemplateRendererBaseConstructorArgs(RevolutionaryStuffService.RevolutionaryStuffServiceConstrutorArgs BaseConstructorArgs);
