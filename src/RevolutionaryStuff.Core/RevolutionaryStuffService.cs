using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core;

public abstract class RevolutionaryStuffService : LoggingDisposableBase
{
    public sealed record RevolutionaryStuffServiceConstrutorArge(ILoggerFactory LoggerFactory)
    { }

    protected RevolutionaryStuffService(RevolutionaryStuffServiceConstrutorArge constructorArgs)
        : base(constructorArgs.LoggerFactory)
    {
    }
}
