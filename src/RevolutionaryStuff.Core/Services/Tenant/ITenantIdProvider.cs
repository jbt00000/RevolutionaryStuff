namespace RevolutionaryStuff.Core.Services.Tenant;

public interface ITenantIdProvider
{
    Task<string> GetTenantIdAsync();
}


