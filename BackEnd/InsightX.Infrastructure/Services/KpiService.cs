using System.Threading;
using InsightX.Application.Common;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
using InsightX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Services
{
    public class KpiService : IKpiService
    {
        private readonly AppDbContext _context;

        public KpiService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<KpiResponseDto>> CreateAsync(CreateKpiDto dto, int companyId, CancellationToken cancellationToken = default)
        {
            var exists = await _context.KPIs
                .AnyAsync(k => k.CompanyId == companyId && k.Name.ToLower() == dto.Name.ToLower(), cancellationToken);

            if (exists)
            {
                return ServiceResult<KpiResponseDto>.Fail(400, "A KPI with this name already exists in your company.");
            }

            var kpi = new KPI
            {
                Name = dto.Name,
                Threshold = dto.Threshold,
                Unit = dto.Unit,
                AlertPercentageDiff = dto.AlertPercentageDiff,
                TrendMonthsCount = dto.TrendMonthsCount,
                ThresholdDirection = dto.ThresholdDirection,
                CompanyId = companyId,
                DepartmentId = dto.DepartmentId,
                Description = dto.Description
            };

            _context.KPIs.Add(kpi);
            await _context.SaveChangesAsync(cancellationToken);

            var deptName = kpi.DepartmentId.HasValue ? await _context.Departments.Where(d => d.Id == kpi.DepartmentId).Select(d => d.Name).FirstOrDefaultAsync(cancellationToken) : null;
            var response = new KpiResponseDto(kpi.Id, kpi.Name, kpi.Threshold, kpi.Unit, kpi.CompanyId, kpi.AlertPercentageDiff, kpi.TrendMonthsCount, kpi.ThresholdDirection, kpi.DepartmentId, deptName, kpi.Description);
            return ServiceResult<KpiResponseDto>.Success(response);
        }

        public async Task<ServiceResult<KpiResponseDto>> GetByIdAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            var kpi = await _context.KPIs
                .Include(k => k.Department)
                .FirstOrDefaultAsync(k => k.Id == id && k.CompanyId == companyId, cancellationToken);

            if (kpi == null)
            {
                return ServiceResult<KpiResponseDto>.Fail(404, "KPI not found.");
            }

            var response = new KpiResponseDto(kpi.Id, kpi.Name, kpi.Threshold, kpi.Unit, kpi.CompanyId, kpi.AlertPercentageDiff, kpi.TrendMonthsCount, kpi.ThresholdDirection, kpi.DepartmentId, kpi.Department?.Name, kpi.Description);
            return ServiceResult<KpiResponseDto>.Success(response);
        }

        public async Task<ServiceResult<List<KpiResponseDto>>> GetAllAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var kpis = await _context.KPIs
                .Include(k => k.Department)
                .Where(k => k.CompanyId == companyId)
                .Select(k => new KpiResponseDto(k.Id, k.Name, k.Threshold, k.Unit, k.CompanyId, k.AlertPercentageDiff, k.TrendMonthsCount, k.ThresholdDirection, k.DepartmentId, k.Department != null ? k.Department.Name : null, k.Description))
                .ToListAsync(cancellationToken);

            return ServiceResult<List<KpiResponseDto>>.Success(kpis);
        }

        public async Task<ServiceResult<KpiResponseDto>> UpdateAsync(int id, UpdateKpiDto dto, int companyId, CancellationToken cancellationToken = default)
        {
            var kpi = await _context.KPIs
                .FirstOrDefaultAsync(k => k.Id == id && k.CompanyId == companyId, cancellationToken);

            if (kpi == null)
            {
                return ServiceResult<KpiResponseDto>.Fail(404, "KPI not found.");
            }

            var exists = await _context.KPIs
                .AnyAsync(k => k.CompanyId == companyId && k.Id != id && k.Name.ToLower() == dto.Name.ToLower(), cancellationToken);

            if (exists)
            {
                return ServiceResult<KpiResponseDto>.Fail(400, "A KPI with this name already exists in your company.");
            }

            kpi.Name = dto.Name;
            kpi.Threshold = dto.Threshold;
            kpi.Unit = dto.Unit;
            kpi.AlertPercentageDiff = dto.AlertPercentageDiff;
            kpi.TrendMonthsCount = dto.TrendMonthsCount;
            kpi.ThresholdDirection = dto.ThresholdDirection;
            kpi.DepartmentId = dto.DepartmentId;
            kpi.Description = dto.Description;

            _context.KPIs.Update(kpi);
            await _context.SaveChangesAsync(cancellationToken);

            var deptName = kpi.DepartmentId.HasValue ? await _context.Departments.Where(d => d.Id == kpi.DepartmentId).Select(d => d.Name).FirstOrDefaultAsync(cancellationToken) : null;
            var response = new KpiResponseDto(kpi.Id, kpi.Name, kpi.Threshold, kpi.Unit, kpi.CompanyId, kpi.AlertPercentageDiff, kpi.TrendMonthsCount, kpi.ThresholdDirection, kpi.DepartmentId, deptName, kpi.Description);
            return ServiceResult<KpiResponseDto>.Success(response);
        }

        public async Task<ServiceResult> DeleteAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            var kpi = await _context.KPIs
                .FirstOrDefaultAsync(k => k.Id == id && k.CompanyId == companyId, cancellationToken);

            if (kpi == null)
            {
                return ServiceResult.Fail(404, "KPI not found.");
            }

            var metrics = await _context.ExtractedMetrics
                .Where(m => m.CompanyId == companyId && m.KPIName.ToLower() == kpi.Name.ToLower())
                .ToListAsync(cancellationToken);

            var alerts = await _context.Alerts
                .Where(a => a.CompanyId == companyId && a.KPIName.ToLower() == kpi.Name.ToLower())
                .ToListAsync(cancellationToken);

            if (metrics.Any())
            {
                _context.ExtractedMetrics.RemoveRange(metrics);
            }
            if (alerts.Any())
            {
                _context.Alerts.RemoveRange(alerts);
            }

            _context.KPIs.Remove(kpi);
            await _context.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success();
        }
    }
}
