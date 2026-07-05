using System.Threading;
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

        public async Task<ServiceResult<CompanyProfileDto>> GetMyCompanyAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var company = await _context.Companies
                .Include(c => c.Departments)
                .Include(c => c.KPIs)
                .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);

            if (company == null)
            {
                return ServiceResult<CompanyProfileDto>.Fail(404, "Company not found.");
            }

            var dto = new CompanyProfileDto(
                company.Id,
                company.Name,
                company.CreatedAt,
                company.Departments.Select(d => new DepartmentProfileDto(d.Id, d.Name)).ToList(),
                company.KPIs.Select(k => new KpiProfileDto(k.Id, k.Name, k.Threshold, k.Unit, k.AlertPercentageDiff, k.TrendMonthsCount, k.ThresholdDirection)).ToList()
            );

            return ServiceResult<CompanyProfileDto>.Success(dto);
        }

        public async Task<ServiceResult> SetupAsync(SetupDto dto, int companyId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.KPIs
                .Where(k => k.CompanyId == companyId)
                .ToListAsync(cancellationToken);

            _context.KPIs.RemoveRange(existing);

            var newKpis = dto.KPIs.Select(k => new KPI
            {
                Name = k.Name,
                Threshold = k.Threshold,
                Unit = k.Unit,
                AlertPercentageDiff = k.AlertPercentageDiff,
                TrendMonthsCount = k.TrendMonthsCount,
                ThresholdDirection = k.ThresholdDirection,
                CompanyId = companyId
            }).ToList();

            await _context.KPIs.AddRangeAsync(newKpis, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success();
        }
    }
}
