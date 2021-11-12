namespace RevolutionaryStuff.Core.ApplicationParts;

public interface ITenantFinder<TTenantId>
{
    Task<TTenantId> GetTenantIdAsync();
}
