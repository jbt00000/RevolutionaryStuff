using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.ApiCore.Services;

public abstract class ApiService(ApiService.ApiServiceConstructorArgs _constructorArgs, ILogger logger)
    : LoggingDisposableBase(logger)
{
    public sealed record ApiServiceConstructorArgs()
    { }
}
