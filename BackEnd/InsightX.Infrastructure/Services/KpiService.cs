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
    public class KpiService : IKpiService
    {
        private readonly AppDbContext _context;

        public KpiService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<KpiResponseDto>> CreateAsync(CreateKpiDto dto, int companyId)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Unit))
            {
                return ServiceResult<KpiResponseDto>.Fail(400, "Invalid KPI name or unit.");
            }

            var exists = await _context.KPIs
                .AnyAsync(k => k.CompanyId == companyId && k.Name.ToLower() == dto.Name.ToLower());

            if (exists)
            {
                return ServiceResult<KpiResponseDto>.Fail(400, "A KPI with this name already exists in your company.");
            }

            var kpi = new KPI
            {
                Name = dto.Name,
                Threshold = dto.Threshold,
                Unit = dto.Unit,
                CompanyId = companyId
            };

            _context.KPIs.Add(kpi);
            await _context.SaveChangesAsync();

            var response = new KpiResponseDto(kpi.Id, kpi.Name, kpi.Threshold, kpi.Unit, kpi.CompanyId);
            return ServiceResult<KpiResponseDto>.Success(response);
        }

        public async Task<ServiceResult<KpiResponseDto>> GetByIdAsync(int id, int companyId)
        {
            var kpi = await _context.KPIs
                .FirstOrDefaultAsync(k => k.Id == id && k.CompanyId == companyId);

            if (kpi == null)
            {
                return ServiceResult<KpiResponseDto>.Fail(404, "KPI not found.");
            }

            var response = new KpiResponseDto(kpi.Id, kpi.Name, kpi.Threshold, kpi.Unit, kpi.CompanyId);
            return ServiceResult<KpiResponseDto>.Success(response);
        }

        public async Task<ServiceResult<List<KpiResponseDto>>> GetAllAsync(int companyId)
        {
            var kpis = await _context.KPIs
                .Where(k => k.CompanyId == companyId)
                .Select(k => new KpiResponseDto(k.Id, k.Name, k.Threshold, k.Unit, k.CompanyId))
                .ToListAsync();

            return ServiceResult<List<KpiResponseDto>>.Success(kpis);
        }

        public async Task<ServiceResult<KpiResponseDto>> UpdateAsync(int id, CreateKpiDto dto, int companyId)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Unit))
            {
                return ServiceResult<KpiResponseDto>.Fail(400, "Invalid KPI name or unit.");
            }

            var kpi = await _context.KPIs
                .FirstOrDefaultAsync(k => k.Id == id && k.CompanyId == companyId);

            if (kpi == null)
            {
                return ServiceResult<KpiResponseDto>.Fail(404, "KPI not found.");
            }

            var exists = await _context.KPIs
                .AnyAsync(k => k.CompanyId == companyId && k.Id != id && k.Name.ToLower() == dto.Name.ToLower());

            if (exists)
            {
                return ServiceResult<KpiResponseDto>.Fail(400, "A KPI with this name already exists in your company.");
            }

            kpi.Name = dto.Name;
            kpi.Threshold = dto.Threshold;
            kpi.Unit = dto.Unit;

            _context.KPIs.Update(kpi);
            await _context.SaveChangesAsync();

            var response = new KpiResponseDto(kpi.Id, kpi.Name, kpi.Threshold, kpi.Unit, kpi.CompanyId);
            return ServiceResult<KpiResponseDto>.Success(response);
        }

        public async Task<ServiceResult> DeleteAsync(int id, int companyId)
        {
            var kpi = await _context.KPIs
                .FirstOrDefaultAsync(k => k.Id == id && k.CompanyId == companyId);

            if (kpi == null)
            {
                return ServiceResult.Fail(404, "KPI not found.");
            }

            _context.KPIs.Remove(kpi);
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }
    }
}
