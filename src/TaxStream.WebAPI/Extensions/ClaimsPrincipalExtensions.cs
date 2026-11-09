using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaxStream.WebAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(sub, out var userId))
            return userId;
        throw new InvalidOperationException("UserId claim is missing or invalid.");
    }
}
