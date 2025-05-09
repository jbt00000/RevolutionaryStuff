namespace RevolutionaryStuff.Core.ApplicationParts;

public interface ITenantProvider<TTenantId> : ITenantFinder<TTenantId>
{
    bool HasTenantId { get; }
    TTenantId TenantId { get; set; }
}
