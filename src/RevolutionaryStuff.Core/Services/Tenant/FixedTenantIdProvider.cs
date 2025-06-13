namespace RevolutionaryStuff.Core.Services.Tenant;

public class FixedTenantIdProvider(string TenantId) : ITenantIdProvider
{
    string ITenantIdProvider.GetTenantId()
        => TenantId;
}
