namespace RevolutionaryStuff.Core.Services.Tenant;

public class FixedTenantIdProvider : ITenantIdProvider
{
    private readonly string TenantId;
    public FixedTenantIdProvider(string tenantId)
    {
        Requires.Text(tenantId);
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId), "Tenant ID cannot be null");
    }
    Task<string> ITenantIdProvider.GetTenantIdAsync()
        => Task.FromResult(TenantId);
}
