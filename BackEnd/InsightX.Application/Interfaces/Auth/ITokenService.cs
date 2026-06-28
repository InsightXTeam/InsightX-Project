using System.Security.Claims;
using InsightX.Domain.Entities.Auth;

namespace InsightX.Application.Interfaces.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
