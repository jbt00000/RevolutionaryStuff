namespace RevolutionaryStuff.Core.ApplicationParts;

public static class TenantFinderHelpers
{
    private class FixedTenantFinder<TTenantId> : ITenantFinder<TTenantId>
    {
        private readonly TTenantId TenantId;

        public FixedTenantFinder(TTenantId tenantId)
        {
            TenantId = tenantId;
        }

        Task<TTenantId> ITenantFinder<TTenantId>.GetTenantIdAsync()
        {
            return Task.FromResult(TenantId);
        }
    }

    public static ITenantFinder<TTenantId> CreateFixedTenantFinder<TTenantId>(TTenantId tenantId)
    {
        return new FixedTenantFinder<TTenantId>(tenantId);
    }
}
