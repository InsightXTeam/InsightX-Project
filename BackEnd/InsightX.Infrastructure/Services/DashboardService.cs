using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Enums;
using InsightX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DashboardKpiDto>> GetKpisSummaryAsync(int companyId, int? departmentId)
        {
            var kpisQuery = _context.KPIs.Where(k => k.CompanyId == companyId);
            
            var metricsQuery = _context.ExtractedMetrics
                .Include(m => m.Report)
                .Where(m => m.CompanyId == companyId && m.ConfirmedByManager && m.Value != null);
                
            if (departmentId.HasValue)
            {
                metricsQuery = metricsQuery.Where(m => m.Report.DepartmentId == departmentId.Value);
            }

            var kpis = await kpisQuery.ToListAsync();
            var latestMetrics = await metricsQuery
                .GroupBy(m => m.KPIName)
                .Select(g => g.OrderByDescending(m => m.Year).ThenByDescending(m => m.Month).FirstOrDefault())
                .ToListAsync();

            var result = new List<DashboardKpiDto>();
            foreach (var kpi in kpis)
            {
                var metric = latestMetrics.FirstOrDefault(m => m?.KPIName == kpi.Name);
                decimal currentValue = (decimal)(metric?.Value ?? 0);
                
                string status = "Good";
                if (kpi.ThresholdDirection == ThresholdDirection.Below)
                {
                    // Lower is better
                    if ((double)currentValue > kpi.Threshold) status = "Warning";
                    if ((double)currentValue > kpi.Threshold * 1.2) status = "Critical";
                }
                else
                {
                    // Higher is better
                    if ((double)currentValue < kpi.Threshold) status = "Warning";
                    if ((double)currentValue < kpi.Threshold * 0.8) status = "Critical";
                }

                result.Add(new DashboardKpiDto
                {
                    Name = kpi.Name,
                    CurrentValue = currentValue,
                    Threshold = kpi.Threshold,
                    Unit = kpi.Unit,
                    Status = status,
                    ThresholdDirection = kpi.ThresholdDirection.ToString()
                });
            }

            return result;
        }

        public async Task<List<DashboardTrendDto>> GetTrendsAsync(int companyId, int? departmentId)
        {
            var metricsQuery = _context.ExtractedMetrics
                .Include(m => m.Report)
                .Where(m => m.CompanyId == companyId && m.ConfirmedByManager && m.Value != null);
                
            if (departmentId.HasValue)
            {
                metricsQuery = metricsQuery.Where(m => m.Report.DepartmentId == departmentId.Value);
            }
            
            // Get last 6 months
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var targetYear = sixMonthsAgo.Year;
            var targetMonth = sixMonthsAgo.Month;
            
            var metrics = await metricsQuery
                .Where(m => m.Year > targetYear || (m.Year == targetYear && m.Month >= targetMonth))
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToListAsync();

            var grouped = metrics.GroupBy(m => m.KPIName);
            var result = new List<DashboardTrendDto>();
            
            foreach(var group in grouped)
            {
                // Note: For a manager, we have exactly 1 value per month.
                // For an owner, we need to average or sum the values across departments. 
                // Let's do Average for now as a default aggregation.
                var aggregatedByMonth = group.GroupBy(x => new { x.Year, x.Month })
                    .Select(g => new { 
                        g.Key.Year, 
                        g.Key.Month, 
                        Value = (decimal)(g.Average(m => m.Value!) ?? 0) 
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToList();

                result.Add(new DashboardTrendDto
                {
                    KPIName = group.Key,
                    Labels = aggregatedByMonth.Select(m => $"{m.Month}/{m.Year}").ToList(),
                    Values = aggregatedByMonth.Select(m => m.Value).ToList()
                });
            }

            return result;
        }

        public async Task<List<DashboardDepartmentPerformanceDto>> GetDepartmentsPerformanceAsync(int companyId)
        {
            var departments = await _context.Departments
                .Where(d => d.CompanyId == companyId)
                .ToListAsync();

            var kpis = await _context.KPIs.Where(k => k.CompanyId == companyId).ToListAsync();
            var metrics = await _context.ExtractedMetrics
                .Include(m => m.Report)
                .Where(m => m.CompanyId == companyId && m.ConfirmedByManager && m.Value != null)
                .ToListAsync();

            var result = new List<DashboardDepartmentPerformanceDto>();

            foreach(var dept in departments)
            {
                var deptMetrics = metrics.Where(m => m.Report.DepartmentId == dept.Id)
                    .GroupBy(m => m.KPIName)
                    .Select(g => g.OrderByDescending(m => m.Year).ThenByDescending(m => m.Month).FirstOrDefault())
                    .ToList();

                int good = 0, warning = 0, critical = 0;

                foreach(var kpi in kpis)
                {
                    var metric = deptMetrics.FirstOrDefault(m => m?.KPIName == kpi.Name);
                    if (metric == null) continue;

                    decimal val = (decimal)metric.Value!;
                    if (kpi.ThresholdDirection == ThresholdDirection.Below)
                    {
                        // Lower is better
                        if ((double)val <= kpi.Threshold) good++;
                        else if ((double)val <= kpi.Threshold * 1.2) warning++;
                        else critical++;
                    }
                    else
                    {
                        // Higher is better
                        if ((double)val >= kpi.Threshold) good++;
                        else if ((double)val >= kpi.Threshold * 0.8) warning++;
                        else critical++;
                    }
                }

                result.Add(new DashboardDepartmentPerformanceDto
                {
                    DepartmentId = dept.Id,
                    DepartmentName = dept.Name,
                    GoodKPIsCount = good,
                    WarningKPIsCount = warning,
                    CriticalKPIsCount = critical,
                    OverallStatus = critical > 0 ? "Critical" : (warning > 0 ? "Warning" : "Good")
                });
            }

            return result;
        }

        public async Task<List<AlertDto>> GetRecentAlertsAsync(int companyId, int? departmentId)
        {
            var query = _context.Alerts
                .Where(a => a.CompanyId == companyId && !a.SeenByOwner);

            if (departmentId.HasValue)
            {
                query = query.Where(a => a.DepartmentId == departmentId.Value);
            }

            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            return alerts.Select(a => new AlertDto
            {
                Id = a.Id,
                KPIName = a.KPIName,
                Message = a.Message,
                Recommendation = a.Recommendation,
                CurrentValue = a.CurrentValue,
                Threshold = a.Threshold,
                AlertType = a.AlertType,
                CreatedAt = a.CreatedAt
            }).ToList();
        }
    }
}
