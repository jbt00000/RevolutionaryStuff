namespace RevolutionaryStuff.ApiCore.Services;

public abstract class ApiService(ApiService.ApiServiceConstructorArgs _constructorArgs)
    : RevolutionaryStuffService(_constructorArgs.BaseConstrutorArgs)
{
    public sealed record ApiServiceConstructorArgs(RevolutionaryStuffServiceConstrutorArge BaseConstrutorArgs)
    { }
}
