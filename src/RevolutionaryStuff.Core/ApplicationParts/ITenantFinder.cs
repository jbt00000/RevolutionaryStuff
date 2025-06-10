namespace RevolutionaryStuff.Core.ApplicationParts;

[Obsolete("Use ITenantIdProvider instead. This interface will be removed in a future version.", false)]
public interface ITenantFinder<TTenantId>
{
    Task<TTenantId> GetTenantIdAsync();
}
