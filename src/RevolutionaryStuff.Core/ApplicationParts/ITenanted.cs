namespace RevolutionaryStuff.Core.ApplicationParts;

[Obsolete("Use ITenantIdHolder instead. This interface will be removed in a future version.", false)]
public interface ITenanted<TTenantId>
{
    TTenantId TenantId { get; set; }
}
