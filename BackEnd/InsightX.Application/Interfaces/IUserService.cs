using System.Collections.Generic;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult> InviteAsync(InviteUserDto dto, int companyId);
        Task<ServiceResult<List<UserResponseDto>>> GetUsersAsync(int companyId, string role, string? departmentIdClaim);
        Task<ServiceResult<List<OwnerManagementDto>>> GetOwnersForManagementAsync();
        Task<ServiceResult> SetActivationStatusAsync(string id, bool isActivated);
        Task<ServiceResult> DeleteManagerAsync(string id, int companyId);
        Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<ServiceResult> UpdateUserDepartmentAsync(string id, int? departmentId, int companyId);
    }
}
