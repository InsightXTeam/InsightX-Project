using System.Threading;
using InsightX.Application.Common;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
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

        public async Task<ServiceResult> InviteAsync(InviteUserDto dto, int companyId, CancellationToken cancellationToken = default)
        {
            var dept = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == dto.DepartmentId && d.CompanyId == companyId, cancellationToken);

            if (dept == null)
            {
                return ServiceResult.Fail(400, "Invalid department for your company.");
            }

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return ServiceResult.Fail(400, "A user with this email already exists.");
            }

            var existingManagers = await _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, RoleName = r.Name })
                .Where(x => x.u.CompanyId == companyId
                         && x.u.DepartmentId == dto.DepartmentId
                         && x.RoleName == "Manager")
                .Select(x => x.u)
                .ToListAsync(cancellationToken);

            foreach (var manager in existingManagers)
            {
                manager.DepartmentId = null;
            }

            if (existingManagers.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                CompanyId = companyId,
                DepartmentId = dto.DepartmentId,
                IsActivated = true,
                MustChangePassword = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult.Fail(400, errors);
            }

            if (!await _roleManager.RoleExistsAsync("Manager"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Manager"));
            }
            await _userManager.AddToRoleAsync(user, "Manager");

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<List<UserResponseDto>>> GetUsersAsync(int companyId, string role, string? departmentIdClaim, CancellationToken cancellationToken = default)
        {
            IQueryable<ApplicationUser> query = _context.Users
                .IgnoreQueryFilters()
                .Where(u => u.CompanyId == companyId)
                .Include(u => u.Department);

            if (role == "Manager")
            {
                if (string.IsNullOrEmpty(departmentIdClaim) || !int.TryParse(departmentIdClaim, out var deptId))
                {
                    return ServiceResult<List<UserResponseDto>>.Fail(403, "Forbidden");
                }
                query = query.Where(u => u.DepartmentId == deptId && !u.IsDeleted);
            }
            else if (role != "Owner")
            {
                return ServiceResult<List<UserResponseDto>>.Fail(403, "Forbidden");
            }

            var rawUsers = await query.ToListAsync(cancellationToken);
            var userIds = rawUsers.Select(u => u.Id).ToList();

            var userRoles = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where userIds.Contains(ur.UserId)
                                   select new { ur.UserId, RoleName = r.Name })
                                  .ToListAsync(cancellationToken);

            var rolesDict = userRoles
                .GroupBy(ur => ur.UserId)
                .ToDictionary(g => g.Key, g => g.First().RoleName);

            var dtos = rawUsers.Select(u => new UserResponseDto(
                u.Id,
                u.Name,
                u.Email ?? string.Empty,
                rolesDict.TryGetValue(u.Id, out var roleName) ? roleName : string.Empty,
                u.DepartmentId,
                u.Department != null ? u.Department.Name : null,
                u.IsDeleted
            )).ToList();

            return ServiceResult<List<UserResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<List<OwnerManagementDto>>> GetOwnersForManagementAsync(CancellationToken cancellationToken = default)
        {
            var owners = await _userManager.GetUsersInRoleAsync("Owner");
            var companyIds = owners.Select(u => u.CompanyId).Distinct().ToList();

            var companies = await _context.Companies
                .Where(c => companyIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken);

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

        public async Task<ServiceResult> SetActivationStatusAsync(string id, bool isActivated, CancellationToken cancellationToken = default)
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

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Owner"))
            {
                var managersInCompany = await _context.Users
                    .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                    .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, RoleName = r.Name })
                    .Where(x => x.u.CompanyId == user.CompanyId && x.RoleName == "Manager")
                    .Select(x => x.u)
                    .ToListAsync(cancellationToken);

                foreach (var manager in managersInCompany)
                {
                    manager.IsActivated = isActivated;
                    await _userManager.UpdateAsync(manager);
                }
            }

            if (!isActivated)
            {
                var userIdsToRevoke = new List<string> { user.Id };
                if (roles.Contains("Owner"))
                {
                    var managerIds = await _context.Users
                        .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                        .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, RoleName = r.Name })
                        .Where(x => x.u.CompanyId == user.CompanyId && x.RoleName == "Manager")
                        .Select(x => x.u.Id)
                        .ToListAsync(cancellationToken);
                    userIdsToRevoke.AddRange(managerIds);
                }

                var activeTokens = await _context.RefreshTokens
                    .Where(r => userIdsToRevoke.Contains(r.UserId) && !r.IsRevoked)
                    .ToListAsync(cancellationToken);

                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteManagerAsync(string id, int companyId, CancellationToken cancellationToken = default)
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

            if (user.CompanyId != companyId)
            {
                return ServiceResult.Fail(403, "You do not have permission to delete this user.");
            }

            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            if (!isManager)
            {
                return ServiceResult.Fail(400, "Only users with the Manager role can be deleted.");
            }

            user.IsDeleted = true;
            user.IsActivated = false;
            user.DepartmentId = null;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return ServiceResult.Fail(400, errors);
            }

            var activeTokens = await _context.RefreshTokens
                .Where(r => r.UserId == id && !r.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
            }
            await _context.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> RestoreManagerAsync(string id, int companyId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                return ServiceResult.Fail(400, "Invalid user ID.");
            }

            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user == null)
            {
                return ServiceResult.Fail(404, "User not found.");
            }

            if (user.CompanyId != companyId)
            {
                return ServiceResult.Fail(403, "You do not have permission to restore this user.");
            }

            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            if (!isManager)
            {
                return ServiceResult.Fail(400, "Only users with the Manager role can be restored.");
            }

            user.IsDeleted = false;
            user.IsActivated = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return ServiceResult.Fail(400, errors);
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
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
            if (user.MustChangePassword)
            {
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);
            }
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> UpdateUserDepartmentAsync(string id, int? departmentId, int companyId, CancellationToken cancellationToken = default)
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

            if (user.CompanyId != companyId)
            {
                return ServiceResult.Fail(403, "You do not have permission to update this user.");
            }

            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            if (!isManager)
            {
                return ServiceResult.Fail(400, "Only users with the Manager role can be assigned to a department.");
            }

            if (departmentId.HasValue)
            {
                var deptExists = await _context.Departments
                    .AnyAsync(d => d.Id == departmentId.Value && d.CompanyId == companyId, cancellationToken);

                if (!deptExists)
                {
                    return ServiceResult.Fail(400, "Invalid department for your company.");
                }

                // Unassign any other manager currently assigned to this department
                var otherManagers = await (from u in _context.Users
                                           join ur in _context.UserRoles on u.Id equals ur.UserId
                                           join r in _context.Roles on ur.RoleId equals r.Id
                                           where u.CompanyId == companyId && u.DepartmentId == departmentId.Value && u.Id != id && r.Name == "Manager"
                                           select u)
                                          .ToListAsync(cancellationToken);

                foreach (var other in otherManagers)
                {
                    other.DepartmentId = null;
                }
            }

            user.DepartmentId = departmentId;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return ServiceResult.Fail(400, errors);
            }

            return ServiceResult.Success();
        }
    }
}
