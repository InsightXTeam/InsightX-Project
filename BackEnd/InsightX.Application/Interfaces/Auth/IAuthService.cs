using InsightX.Application.Common;
using InsightX.Application.DTOs.Auth;

namespace InsightX.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<ServiceResult<object>> RegisterAsync(RegisterDto dto);

        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto);

        Task<ServiceResult<AuthResponseDto>> RefreshAsync(RefreshDto dto);

        Task<ServiceResult> LogoutAsync(string userId);
    }
}