namespace RevolutionaryStuff.Core.Services.Tenant;

public class CrossTenantException : InvalidOperationException
{
    public readonly object ExpectedTenantId;
    public readonly object ActualTenantId;

    public static void ThrowIfCrossTenant(ITenantIdHolder expectedTenantIdHolder, ITenantIdHolder actualTenantIdHolder)
        => ThrowIfCrossTenant(expectedTenantIdHolder?.TenantId, actualTenantIdHolder?.TenantId);

    public static void ThrowIfCrossTenant(object expectedTenantId, object actualTenantId)
    {
        if (!Equals(expectedTenantId, actualTenantId))
        {
            throw new CrossTenantException(expectedTenantId, actualTenantId);
        }
    }

    public CrossTenantException(object expectedTenantId, object newTenantId)
        : base($"expectedTenantId={expectedTenantId} actualTenantId={newTenantId}")
    {
        ExpectedTenantId = expectedTenantId;
        ActualTenantId = newTenantId;
    }
}
