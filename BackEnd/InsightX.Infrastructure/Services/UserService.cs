using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs.Auth;
using InsightX.Application.Interfaces.Auth;
using InsightX.Domain.Entities.Auth;
using InsightX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<ServiceResult> InviteAsync(InviteUserDto dto, int companyId)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return ServiceResult.Fail(400, "Invalid invitation data.");
            }

            // Verify department belongs to the caller's company (cross-company assignment check)
            var dept = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == dto.DepartmentId && d.CompanyId == companyId);

            if (dept == null)
            {
                return ServiceResult.Fail(400, "Invalid department for your company.");
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return ServiceResult.Fail(400, "A user with this email already exists.");
            }

            // Create ApplicationUser under same company and department
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                CompanyId = companyId,
                DepartmentId = dto.DepartmentId,
                IsActivated = true
            };

            // Attempt user creation
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult.Fail(400, errors);
            }

            // Assign Manager role
            if (!await _roleManager.RoleExistsAsync("Manager"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Manager"));
            }
            await _userManager.AddToRoleAsync(user, "Manager");

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<List<UserResponseDto>>> GetUsersAsync(int companyId, string role, string? departmentIdClaim)
        {
            IQueryable<ApplicationUser> query = _context.Users
                .Where(u => u.CompanyId == companyId)
                .Include(u => u.Department);

            // Filter users based on Caller Role
            if (role == "Manager")
            {
                if (string.IsNullOrEmpty(departmentIdClaim))
                {
                    return ServiceResult<List<UserResponseDto>>.Fail(403, "Forbidden");
                }
                var deptId = int.Parse(departmentIdClaim);
                query = query.Where(u => u.DepartmentId == deptId);
            }
            else if (role != "Owner")
            {
                return ServiceResult<List<UserResponseDto>>.Fail(403, "Forbidden");
            }

            // Map strictly to UserResponseDto by loading users first then joining role details
            var rawUsers = await query.ToListAsync();
            var userIds = rawUsers.Select(u => u.Id).ToList();

            var userRoles = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where userIds.Contains(ur.UserId)
                                   select new { ur.UserId, RoleName = r.Name })
                                  .ToListAsync();

            var rolesDict = userRoles.ToDictionary(ur => ur.UserId, ur => ur.RoleName);

            var dtos = rawUsers.Select(u => new UserResponseDto(
                u.Id,
                u.Name,
                u.Email ?? string.Empty,
                rolesDict.TryGetValue(u.Id, out var roleName) ? roleName : string.Empty,
                u.DepartmentId,
                u.Department != null ? u.Department.Name : null
            )).ToList();

            return ServiceResult<List<UserResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<List<OwnerManagementDto>>> GetOwnersForManagementAsync()
        {
            var owners = await _userManager.GetUsersInRoleAsync("Owner");
            var companyIds = owners.Select(u => u.CompanyId).Distinct().ToList();

            var companies = await _context.Companies
                .Where(c => companyIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id);

            var dtos = owners.Select(u => new OwnerManagementDto(
                u.Id,
                u.Name,
                u.Email ?? string.Empty,
                u.IsActivated,
                u.CompanyId,
                companies.TryGetValue(u.CompanyId, out var c) ? c.Name : string.Empty,
                companies.TryGetValue(u.CompanyId, out var comp) ? comp.CreatedAt : System.DateTime.MinValue
            )).ToList();

            return ServiceResult<List<OwnerManagementDto>>.Success(dtos);
        }

        public async Task<ServiceResult> SetActivationStatusAsync(string id, bool isActivated)
        {
            if (string.IsNullOrEmpty(id))
            {
                return ServiceResult.Fail(400, "Invalid user ID.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return ServiceResult.Fail(404, "User not found.");
            }

            user.IsActivated = isActivated;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult.Fail(400, errors);
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteManagerAsync(string id, int companyId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return ServiceResult.Fail(400, "Invalid user ID.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return ServiceResult.Fail(404, "User not found.");
            }

            // Verify they belong to the caller's company
            if (user.CompanyId != companyId)
            {
                return ServiceResult.Fail(403, "You do not have permission to delete this user.");
            }

            // Ensure the user being deleted is a Manager (Owners cannot delete other Owners or admins)
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            if (!isManager)
            {
                return ServiceResult.Fail(400, "Only users with the Manager role can be deleted.");
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                return ServiceResult.Fail(400, errors);
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            if (string.IsNullOrEmpty(userId) || dto == null || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return ServiceResult.Fail(400, "Invalid change password request.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult.Fail(404, "User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult.Fail(400, errors);
            }

            return ServiceResult.Success();
        }
    }
}
