using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return ServiceResult<DepartmentResponseDto>.Fail(400, "Invalid department name.");
            }

            // Duplicate check within the same company (case-insensitive)
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

            var responseDto = new DepartmentResponseDto(dept.Id, dept.Name, dept.CompanyId);
            return ServiceResult<DepartmentResponseDto>.Success(responseDto);
        }

        public async Task<ServiceResult<List<DepartmentResponseDto>>> GetAllAsync(int companyId)
        {
            var departments = await _context.Departments
                .Where(d => d.CompanyId == companyId)
                .Select(d => new DepartmentResponseDto(d.Id, d.Name, d.CompanyId))
                .ToListAsync();

            return ServiceResult<List<DepartmentResponseDto>>.Success(departments);
        }
    }
}
