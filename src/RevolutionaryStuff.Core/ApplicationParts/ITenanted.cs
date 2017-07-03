using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface ITenantFinder<TTenantId>
    {
        Task<TTenantId> GetTenantIdAsync();
    }

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

    public interface ITenanted<TTenantId>
    {
        TTenantId TenantId { get; set; }
    }
}
