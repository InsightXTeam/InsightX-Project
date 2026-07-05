using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult> InviteAsync(InviteUserDto dto, int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<UserResponseDto>>> GetUsersAsync(int companyId, string role, string? departmentIdClaim, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<OwnerManagementDto>>> GetOwnersForManagementAsync(CancellationToken cancellationToken = default);
        Task<ServiceResult> SetActivationStatusAsync(string id, bool isActivated, CancellationToken cancellationToken = default);
        Task<ServiceResult> DeleteManagerAsync(string id, int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateUserDepartmentAsync(string id, int? departmentId, int companyId, CancellationToken cancellationToken = default);
    }
}
