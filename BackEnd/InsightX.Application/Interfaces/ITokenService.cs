using System.Security.Claims;
using InsightX.Domain.Entities;

namespace InsightX.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, System.Collections.Generic.IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
