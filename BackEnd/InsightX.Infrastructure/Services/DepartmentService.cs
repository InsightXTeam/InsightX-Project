using InsightX.Application.Common;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
using InsightX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, int companyId)
        {
            var exists = await _context.Departments
                .AnyAsync(d => d.CompanyId == companyId && d.Name.ToLower() == dto.Name.ToLower());

            if (exists)
            {
                return ServiceResult<DepartmentResponseDto>.Fail(400, "A department with this name already exists in your company.");
            }

            var dept = new Department
            {
                Name = dto.Name,
                CompanyId = companyId
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            var responseDto = new DepartmentResponseDto(dept.Id, dept.Name, dept.CompanyId);
            return ServiceResult<DepartmentResponseDto>.Success(responseDto);
        }

        public async Task<ServiceResult<DepartmentResponseDto>> GetByIdAsync(int id, int companyId)
        {
            var dept = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId);

            if (dept == null)
            {
                return ServiceResult<DepartmentResponseDto>.Fail(404, "Department not found.");
            }

            var manager = await (from u in _context.Users
                                 join ur in _context.UserRoles on u.Id equals ur.UserId
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where u.CompanyId == companyId && u.DepartmentId == id && r.Name == "Manager"
                                 select new { u.Id, u.Name })
                                .FirstOrDefaultAsync();

            var responseDto = new DepartmentResponseDto(dept.Id, dept.Name, dept.CompanyId, manager?.Name, manager?.Id);
            return ServiceResult<DepartmentResponseDto>.Success(responseDto);
        }

        public async Task<ServiceResult<List<DepartmentResponseDto>>> GetAllAsync(int companyId)
        {
            var departments = await _context.Departments
                .Where(d => d.CompanyId == companyId)
                .ToListAsync();

            var managers = await (from u in _context.Users
                                  join ur in _context.UserRoles on u.Id equals ur.UserId
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where u.CompanyId == companyId && r.Name == "Manager"
                                  select new { u.DepartmentId, u.Id, u.Name })
                                 .ToListAsync();

            var managerDict = managers
                .Where(m => m.DepartmentId.HasValue)
                .GroupBy(m => m.DepartmentId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var dtos = departments.Select(d => new DepartmentResponseDto(
                d.Id,
                d.Name,
                d.CompanyId,
                managerDict.TryGetValue(d.Id, out var m) ? m.Name : null,
                managerDict.TryGetValue(d.Id, out m) ? m.Id : null
            )).ToList();

            return ServiceResult<List<DepartmentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<DepartmentResponseDto>> UpdateAsync(int id, UpdateDepartmentDto dto, int companyId)
        {
            var dept = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId);

            if (dept == null)
            {
                return ServiceResult<DepartmentResponseDto>.Fail(404, "Department not found.");
            }

            var exists = await _context.Departments
                .AnyAsync(d => d.CompanyId == companyId && d.Id != id && d.Name.ToLower() == dto.Name.ToLower());

            if (exists)
            {
                return ServiceResult<DepartmentResponseDto>.Fail(400, "A department with this name already exists in your company.");
            }

            // Update name
            dept.Name = dto.Name;
            _context.Departments.Update(dept);

            // Handle manager update
            // 1. Unassign any current manager for this department
            var currentManagers = await (from u in _context.Users
                                         join ur in _context.UserRoles on u.Id equals ur.UserId
                                         join r in _context.Roles on ur.RoleId equals r.Id
                                         where u.CompanyId == companyId && u.DepartmentId == id && r.Name == "Manager"
                                         select u)
                                        .ToListAsync();

            foreach (var manager in currentManagers)
            {
                manager.DepartmentId = null;
            }

            // 2. Assign the new manager if provided
            if (!string.IsNullOrEmpty(dto.ManagerId))
            {
                var newManager = await (from u in _context.Users
                                        join ur in _context.UserRoles on u.Id equals ur.UserId
                                        join r in _context.Roles on ur.RoleId equals r.Id
                                        where u.CompanyId == companyId && u.Id == dto.ManagerId && r.Name == "Manager"
                                        select u)
                                       .FirstOrDefaultAsync();

                if (newManager != null)
                {
                    newManager.DepartmentId = id;
                }
            }

            await _context.SaveChangesAsync();

            // Fetch final manager details
            var finalManager = await (from u in _context.Users
                                      join ur in _context.UserRoles on u.Id equals ur.UserId
                                      join r in _context.Roles on ur.RoleId equals r.Id
                                      where u.CompanyId == companyId && u.DepartmentId == id && r.Name == "Manager"
                                      select new { u.Id, u.Name })
                                     .FirstOrDefaultAsync();

            var responseDto = new DepartmentResponseDto(dept.Id, dept.Name, dept.CompanyId, finalManager?.Name, finalManager?.Id);
            return ServiceResult<DepartmentResponseDto>.Success(responseDto);
        }

        public async Task<ServiceResult> DeleteAsync(int id, int companyId)
        {
            var dept = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId);

            if (dept == null)
            {
                return ServiceResult.Fail(404, "Department not found.");
            }

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }
    }
}
