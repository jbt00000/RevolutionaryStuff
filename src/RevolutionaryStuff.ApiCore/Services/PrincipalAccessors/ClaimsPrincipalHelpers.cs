using System.Security.Claims;

namespace RevolutionaryStuff.ApiCore.Services.PrincipalAccessors;

public static class ClaimsPrincipalHelpers
{
    extension(ClaimsPrincipal source)
    {
        public string GetAuthenticatedClaimValue(string claimType)
        {
            if (source?.Identity?.IsAuthenticated ?? false)
            {
                var value = source.FindFirst(claimType)?.Value;
                return string.IsNullOrEmpty(value) ? throw new InvalidOperationException($"Caller does not have a {claimType}") : value;
            }
            throw new InvalidOperationException("Caller is not authenticated");
        }

        public string GetAuthenticatedClaimValueSubject()
            => source.GetAuthenticatedClaimValue(ClaimTypes.NameIdentifier);
    }
}
