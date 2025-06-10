namespace RevolutionaryStuff.Core.ApplicationParts;

[Obsolete("This... will just plain out die in a future version. This is a combination between ITenantIdProvider and ITenantIdHolder.", false)]
public interface ITenantProvider<TTenantId> : ITenantFinder<TTenantId>
{
    bool HasTenantId { get; }
    TTenantId TenantId { get; set; }
}
