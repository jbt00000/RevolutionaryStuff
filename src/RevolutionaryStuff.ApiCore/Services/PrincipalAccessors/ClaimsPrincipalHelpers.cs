using System.Security.Claims;

namespace RevolutionaryStuff.ApiCore.Services.PrincipalAccessors;

public static class ClaimsPrincipalHelpers
{
    public static string GetAuthenticatedClaimValue(this ClaimsPrincipal cp, string claimType)
    {
        if (cp?.Identity?.IsAuthenticated ?? false)
        {
            var value = cp.FindFirst(claimType)?.Value;
            return string.IsNullOrEmpty(value) ? throw new InvalidOperationException($"Caller does not have a {claimType}") : value;
        }
        throw new InvalidOperationException("Caller is not authenticated");
    }

    public static string GetAuthenticatedClaimValueSubject(this ClaimsPrincipal cp)
        => cp.GetAuthenticatedClaimValue(ClaimTypes.NameIdentifier);
}
