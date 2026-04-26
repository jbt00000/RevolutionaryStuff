using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core;

public abstract class RevolutionaryStuffServiceBase : LoggingDisposableBase
{
    public sealed record RevolutionaryStuffServiceBaseConstrutorArge(ILoggerFactory LoggerFactory)
    { }

    protected RevolutionaryStuffServiceBase(RevolutionaryStuffServiceBaseConstrutorArge constructorArgs)
        : base(constructorArgs.LoggerFactory)
    {
    }
}
