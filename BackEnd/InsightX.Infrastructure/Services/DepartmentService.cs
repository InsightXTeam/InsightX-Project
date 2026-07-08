using System.Threading;
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
        private readonly IKpiService _kpiService;

        public DepartmentService(AppDbContext context, IKpiService kpiService)
        {
            _context = context;
            _kpiService = kpiService;
        }

        public async Task<ServiceResult<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, int companyId, CancellationToken cancellationToken = default)
        {
            var exists = await _context.Departments
                .AnyAsync(d => d.CompanyId == companyId && d.Name.ToLower() == dto.Name.ToLower(), cancellationToken);

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
            await _context.SaveChangesAsync(cancellationToken);

            var responseDto = new DepartmentResponseDto(dept.Id, dept.Name, dept.CompanyId);
            return ServiceResult<DepartmentResponseDto>.Success(responseDto);
        }

        public async Task<ServiceResult<DepartmentResponseDto>> GetByIdAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            var dept = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId, cancellationToken);

            if (dept == null)
            {
                return ServiceResult<DepartmentResponseDto>.Fail(404, "Department not found.");
            }

            var manager = await (from u in _context.Users
                                 join ur in _context.UserRoles on u.Id equals ur.UserId
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where u.CompanyId == companyId && u.DepartmentId == id && r.Name == "Manager"
                                 select new { u.Id, u.Name })
                                .FirstOrDefaultAsync(cancellationToken);

            var responseDto = new DepartmentResponseDto(dept.Id, dept.Name, dept.CompanyId, manager?.Name, manager?.Id);
            return ServiceResult<DepartmentResponseDto>.Success(responseDto);
        }

        public async Task<ServiceResult<List<DepartmentResponseDto>>> GetAllAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var departments = await _context.Departments
                .Where(d => d.CompanyId == companyId)
                .ToListAsync(cancellationToken);

            var managers = await (from u in _context.Users
                                  join ur in _context.UserRoles on u.Id equals ur.UserId
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where u.CompanyId == companyId && r.Name == "Manager"
                                  select new { u.DepartmentId, u.Id, u.Name })
                                 .ToListAsync(cancellationToken);

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

        public async Task<ServiceResult<DepartmentResponseDto>> UpdateAsync(int id, UpdateDepartmentDto dto, int companyId, CancellationToken cancellationToken = default)
        {
            var dept = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId, cancellationToken);

            if (dept == null)
            {
                return ServiceResult<DepartmentResponseDto>.Fail(404, "Department not found.");
            }

            var exists = await _context.Departments
                .AnyAsync(d => d.CompanyId == companyId && d.Id != id && d.Name.ToLower() == dto.Name.ToLower(), cancellationToken);

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
                                        .ToListAsync(cancellationToken);

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
                                       .FirstOrDefaultAsync(cancellationToken);

                if (newManager != null)
                {
                    newManager.DepartmentId = id;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Fetch final manager details
            var finalManager = await (from u in _context.Users
                                      join ur in _context.UserRoles on u.Id equals ur.UserId
                                      join r in _context.Roles on ur.RoleId equals r.Id
                                      where u.CompanyId == companyId && u.DepartmentId == id && r.Name == "Manager"
                                      select new { u.Id, u.Name })
                                     .FirstOrDefaultAsync(cancellationToken);

            var responseDto = new DepartmentResponseDto(dept.Id, dept.Name, dept.CompanyId, finalManager?.Name, finalManager?.Id);
            return ServiceResult<DepartmentResponseDto>.Success(responseDto);
        }

        public async Task<ServiceResult> DeleteAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            var dept = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId, cancellationToken);

            if (dept == null)
            {
                return ServiceResult.Fail(404, "Department not found.");
            }

            var kpis = await _context.KPIs.Where(k => k.DepartmentId == id).ToListAsync(cancellationToken);
            foreach (var kpi in kpis)
            {
                await _kpiService.DeleteAsync(kpi.Id, companyId, cancellationToken);
            }

            var remainingAlerts = await _context.Alerts.Where(a => a.DepartmentId == id).ToListAsync(cancellationToken);
            if (remainingAlerts.Any())
            {
                _context.Alerts.RemoveRange(remainingAlerts);
            }

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success();
        }
    }
}
