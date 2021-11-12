namespace RevolutionaryStuff.Core.ApplicationParts;

public interface ITemplateProcessor
{
    Task<string> ProcessAsync(string template, object model);
}
