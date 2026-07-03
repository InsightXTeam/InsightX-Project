using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(UserProfileDto user);
        string GenerateRefreshToken();
    }
}
