namespace RevolutionaryStuff.Core.Services.Tenant;

public class CrossTenantException : InvalidOperationException
{
    public readonly string ExpectedTenantId;
    public readonly string ActualTenantId;

    public static void ThrowIfCrossTenant(ITenantIdHolder expectedTenantIdHolder, ITenantIdHolder actualTenantIdHolder, bool permitNullActualTenantId = true)
        => ThrowIfCrossTenant(expectedTenantIdHolder?.TenantId, actualTenantIdHolder?.TenantId, permitNullActualTenantId);

    public static void ThrowIfCrossTenant(string expectedTenantId, string actualTenantId, bool permitNullActualTenantId = true)
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

    public CrossTenantException(string expectedTenantId, string actualTenantId)
        : base($"expectedTenantId={expectedTenantId} actualTenantId={actualTenantId}")
    {
        ExpectedTenantId = expectedTenantId;
        ActualTenantId = actualTenantId;
    }
}
