using System;
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
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;

        public CompanyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<CompanyProfileDto>> GetMyCompanyAsync(int companyId)
        {
            var company = await _context.Companies
                .Include(c => c.Departments)
                .Include(c => c.KPIs)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
            {
                return ServiceResult<CompanyProfileDto>.Fail(404, "Company not found.");
            }

            var dto = new CompanyProfileDto(
                company.Id,
                company.Name,
                company.CreatedAt,
                company.Departments.Select(d => new DepartmentProfileDto(d.Id, d.Name)).ToList(),
                company.KPIs.Select(k => new KpiProfileDto(k.Id, k.Name, k.Threshold, k.Unit)).ToList()
            );

            return ServiceResult<CompanyProfileDto>.Success(dto);
        }

        public async Task<ServiceResult> SetupAsync(SetupDto dto, int companyId)
        {
            if (dto == null || dto.KPIs == null)
            {
                return ServiceResult.Fail(400, "Invalid KPIs request.");
            }

            var existing = await _context.KPIs
                .Where(k => k.CompanyId == companyId)
                .ToListAsync();

            _context.KPIs.RemoveRange(existing);

            var newKpis = dto.KPIs.Select(k => new KPI
            {
                Name = k.Name,
                Threshold = k.Threshold,
                Unit = k.Unit,
                CompanyId = companyId
            }).ToList();

            await _context.KPIs.AddRangeAsync(newKpis);

            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }
    }
}
