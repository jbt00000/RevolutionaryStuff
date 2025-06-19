namespace RevolutionaryStuff.Core.Services.Tenant;

public class CrossTenantException : InvalidOperationException
{
    public readonly object ExpectedTenantId;
    public readonly object ActualTenantId;

    public static void ThrowIfCrossTenant(ITenantIdHolder expectedTenantIdHolder, ITenantIdHolder actualTenantIdHolder, bool permitNullActualTenantId = true)
        => ThrowIfCrossTenant(expectedTenantIdHolder?.TenantId, actualTenantIdHolder?.TenantId, permitNullActualTenantId);

    public static void ThrowIfCrossTenant(object expectedTenantId, object actualTenantId, bool permitNullActualTenantId=true)
    {
        if (!Equals(expectedTenantId, actualTenantId))
        {
            if (permitNullActualTenantId && actualTenantId == null)
            {
                return;
            }
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
