namespace RevolutionaryStuff.Core.ApplicationParts;

public interface ITenanted<TTenantId>
{
    TTenantId TenantId { get; set; }
}
