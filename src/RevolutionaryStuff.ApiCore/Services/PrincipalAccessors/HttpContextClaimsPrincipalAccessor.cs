using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RevolutionaryStuff.ApiCore.Services.PrincipalAccessors;

internal class HttpContextClaimsPrincipalAccessor(IHttpContextAccessor _HttpContextAccessor) : IHttpContextClaimsPrincipalAccessor
{
    ClaimsPrincipal? IClaimsPrincipalAccessor.ClaimsPrincipal
        => _HttpContextAccessor.HttpContext?.User;
}
