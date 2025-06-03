namespace RevolutionaryStuff.Core.Services.Tenant;

public class TenantIdMissingException : KeyNotFoundException
{
    public TenantIdMissingException(string reason)
        : base(reason)
    { }

    public static void ThrowIfMissing(string tenantId)
    {
        if (tenantId == null)
        {
            throw new TenantIdMissingException($"Tenant ID cannot be null");
        }
    }
}
