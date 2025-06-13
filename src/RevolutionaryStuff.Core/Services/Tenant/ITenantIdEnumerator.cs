namespace RevolutionaryStuff.Core.Services.Tenant;

public interface ITenantIdEnumerator
{
    Task<IList<string>> GetTenantIdsAsync();

    Task ForEachScopedTenantAsync(Func<ExecuteArgs, Task> executeAsync);

    record ExecuteArgs(IServiceProvider ServiceProvider, string TenantId)
    { }
}
