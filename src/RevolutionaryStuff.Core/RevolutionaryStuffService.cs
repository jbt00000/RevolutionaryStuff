using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core;

public abstract class RevolutionaryStuffService : LoggingDisposableBase
{
    public sealed record RevolutionaryStuffServiceConstrutorArgs(ILoggerFactory LoggerFactory)
    { }

    protected RevolutionaryStuffService(RevolutionaryStuffServiceConstrutorArgs constructorArgs)
        : base(constructorArgs.LoggerFactory)
    {
    }
}
