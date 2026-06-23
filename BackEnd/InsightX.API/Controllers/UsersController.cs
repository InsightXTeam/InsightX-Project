using System.Security.Claims;
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
        public async Task<IActionResult> Invite([FromBody] InviteUserDto dto)
        {
            var companyId = User.GetCompanyId();
            var result = await _userService.InviteAsync(dto, companyId);
            return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var companyId = User.GetCompanyId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var deptIdClaim = User.FindFirst("DepartmentId")?.Value;

            var result = await _userService.GetUsersAsync(companyId, role, deptIdClaim);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet("owners")]
        [Authorize(Roles = "sadmin")]
        public async Task<IActionResult> GetOwners()
        {
            var result = await _userService.GetOwnersForManagementAsync();
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "sadmin")]
        public async Task<IActionResult> Activate(string id)
        {
            var result = await _userService.SetActivationStatusAsync(id, true);
            return result.IsSuccess ? Ok(new { Message = "User activated successfully." }) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "sadmin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            var result = await _userService.SetActivationStatusAsync(id, false);
            return result.IsSuccess ? Ok(new { Message = "User deactivated successfully." }) : StatusCode(result.StatusCode, result.Error);
        }
    }
}
