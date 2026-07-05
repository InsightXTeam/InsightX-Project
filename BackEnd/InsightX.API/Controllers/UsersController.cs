using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Extensions;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("invite")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Invite([FromBody] InviteUserDto dto, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _userService.InviteAsync(dto, companyId, cancellationToken);
            return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var deptIdClaim = User.FindFirst("DepartmentId")?.Value;

            var result = await _userService.GetUsersAsync(companyId, role, deptIdClaim, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet("owners")]
        [Authorize(Roles = "sadmin")]
        public async Task<IActionResult> GetOwners(CancellationToken cancellationToken)
        {
            var result = await _userService.GetOwnersForManagementAsync(cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "sadmin")]
        public async Task<IActionResult> Activate(string id, CancellationToken cancellationToken)
        {
            var result = await _userService.SetActivationStatusAsync(id, true, cancellationToken);
            return result.IsSuccess ? Ok(new { Message = "User activated successfully." }) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "sadmin")]
        public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
        {
            var result = await _userService.SetActivationStatusAsync(id, false, cancellationToken);
            return result.IsSuccess ? Ok(new { Message = "User deactivated successfully." }) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _userService.DeleteManagerAsync(id, companyId, cancellationToken);
            return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await _userService.ChangePasswordAsync(userId, dto, cancellationToken);
            return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPut("{id}/department")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateUserDepartment(string id, [FromBody] UpdateUserDepartmentDto dto, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _userService.UpdateUserDepartmentAsync(id, dto.DepartmentId, companyId, cancellationToken);
            return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result.Error);
        }
    }
}
