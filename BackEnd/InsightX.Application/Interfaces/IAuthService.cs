using System.Threading;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<object>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
        Task<ServiceResult<AuthResponseDto>> RefreshAsync(RefreshDto dto, CancellationToken cancellationToken = default);
        Task<ServiceResult> LogoutAsync(string userId, CancellationToken cancellationToken = default);
    }
}
