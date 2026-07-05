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
            var existingKpis = await _context.KPIs
                .Where(k => k.CompanyId == companyId)
                .ToListAsync(cancellationToken);

            var newNames = dto.KPIs.Select(k => k.Name).ToList();

            var toRemove = existingKpis.Where(k => !newNames.Contains(k.Name)).ToList();
            _context.KPIs.RemoveRange(toRemove);

            foreach (var kpiDto in dto.KPIs)
            {
                var existingKpi = existingKpis.FirstOrDefault(k => k.Name == kpiDto.Name);
                if (existingKpi != null)
                {
                    existingKpi.Threshold = kpiDto.Threshold;
                    existingKpi.Unit = kpiDto.Unit;
                    existingKpi.AlertPercentageDiff = kpiDto.AlertPercentageDiff;
                    existingKpi.TrendMonthsCount = kpiDto.TrendMonthsCount;
                    existingKpi.ThresholdDirection = kpiDto.ThresholdDirection;
                }
                else
                {
                    _context.KPIs.Add(new KPI
                    {
                        Name = kpiDto.Name,
                        Threshold = kpiDto.Threshold,
                        Unit = kpiDto.Unit,
                        AlertPercentageDiff = kpiDto.AlertPercentageDiff,
                        TrendMonthsCount = kpiDto.TrendMonthsCount,
                        ThresholdDirection = kpiDto.ThresholdDirection,
                        CompanyId = companyId
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success();
        }
    }
}
