using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public DashboardService(IAppDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        private static int GetMonthNumber(string month)
        {
            return month.ToLower() switch
            {
                "january" => 1,
                "february" => 2,
                "march" => 3,
                "april" => 4,
                "may" => 5,
                "june" => 6,
                "july" => 7,
                "august" => 8,
                "september" => 9,
                "october" => 10,
                "november" => 11,
                "december" => 12,
                _ => 1
            };
        }

        private bool IsKpiAllowedForUser(string kpiName)
        {
            if (_currentUserService.Role == "Owner") return true;

            var deptId = _currentUserService.DepartmentId;
            return kpiName.ToLower() switch
            {
                "production" => deptId == 1,
                "revenue" => deptId == 1,
                "defect rate" => deptId == 2,
                "absent employees" => deptId == 3,
                _ => false
            };
        }

        private string CalculateKpiStatus(string kpiName, double value, double threshold)
        {
            return kpiName.ToLower() switch
            {
                "production" => value >= threshold ? "Good" : "Critical",
                "revenue" => value >= threshold ? "Good" : "Critical",
                "defect rate" => value <= threshold ? "Good" : "Critical",
                "absent employees" => value <= threshold ? "Good" : "Critical",
                _ => "Good"
            };
        }

        public async Task<List<KpiCardDto>> GetKpiCardsAsync()
        {
            var companyId = _currentUserService.CompanyId;
            var role = _currentUserService.Role;
            var deptId = _currentUserService.DepartmentId;

            // Fetch thresholds
            var thresholds = await _dbContext.KpiThresholds
                .Where(t => t.CompanyId == companyId)
                .ToListAsync();

            // Fetch latest confirmed metrics
            var metricsQuery = _dbContext.ExtractedMetrics
                .Include(m => m.Report)
                .Where(m => m.CompanyId == companyId && m.ConfirmedByManager);

            // Filter by department if Manager
            if (role == "Manager" && deptId.HasValue)
            {
                metricsQuery = metricsQuery.Where(m => m.Report != null && m.Report.DepartmentId == deptId.Value);
            }

            var allMetrics = await metricsQuery.ToListAsync();

            // Group by KPI and find latest by Year and Month
            var latestMetrics = allMetrics
                .GroupBy(m => m.KPIName)
                .Select(g => g
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => GetMonthNumber(m.Month))
                    .First())
                .ToList();

            var cards = new List<KpiCardDto>();

            foreach (var metric in latestMetrics)
            {
                // Double check authorization on KPI name
                if (!IsKpiAllowedForUser(metric.KPIName)) continue;

                var threshold = thresholds.FirstOrDefault(t => t.KPIName == metric.KPIName);
                var thresholdVal = threshold?.ThresholdValue ?? 0;
                var isPct = threshold?.IsPercentage ?? false;

                cards.Add(new KpiCardDto
                {
                    KPIName = metric.KPIName,
                    CurrentValue = metric.Value,
                    Threshold = thresholdVal,
                    IsPercentage = isPct,
                    Status = CalculateKpiStatus(metric.KPIName, metric.Value, thresholdVal)
                });
            }

            return cards;
        }

        public async Task<List<TrendChartDto>> GetTrendsAsync(string kpiName)
        {
            var companyId = _currentUserService.CompanyId;
            var role = _currentUserService.Role;
            var deptId = _currentUserService.DepartmentId;

            if (!IsKpiAllowedForUser(kpiName))
            {
                return new List<TrendChartDto>();
            }

            var metricsQuery = _dbContext.ExtractedMetrics
                .Include(m => m.Report)
                .Where(m => m.CompanyId == companyId && m.KPIName == kpiName && m.ConfirmedByManager);

            if (role == "Manager" && deptId.HasValue)
            {
                metricsQuery = metricsQuery.Where(m => m.Report != null && m.Report.DepartmentId == deptId.Value);
            }

            var metrics = await metricsQuery.ToListAsync();

            return metrics
                .OrderBy(m => m.Year)
                .ThenBy(m => GetMonthNumber(m.Month))
                .Select(m => new TrendChartDto
                {
                    Month = m.Month,
                    Year = m.Year,
                    Value = m.Value
                })
                .ToList();
        }

        public async Task<List<DepartmentSummaryDto>> GetDepartmentSummariesAsync()
        {
            var companyId = _currentUserService.CompanyId;
            var role = _currentUserService.Role;
            var deptId = _currentUserService.DepartmentId;

            // Fetch departments
            var departmentsQuery = _dbContext.Departments.Where(d => d.CompanyId == companyId);
            if (role == "Manager" && deptId.HasValue)
            {
                departmentsQuery = departmentsQuery.Where(d => d.Id == deptId.Value);
            }

            var departments = await departmentsQuery.ToListAsync();
            var thresholds = await _dbContext.KpiThresholds.Where(t => t.CompanyId == companyId).ToListAsync();
            var metrics = await _dbContext.ExtractedMetrics
                .Include(m => m.Report)
                .Where(m => m.CompanyId == companyId && m.ConfirmedByManager)
                .ToListAsync();

            var summaries = new List<DepartmentSummaryDto>();

            foreach (var dept in departments)
            {
                // Map KPI name to department
                string targetKpi = dept.Id switch
                {
                    1 => "Production", // Main KPI for production
                    2 => "Defect Rate",
                    3 => "Absent Employees",
                    _ => "Production"
                };

                // Find latest metric for this KPI and department
                var latestMetric = metrics
                    .Where(m => m.KPIName == targetKpi && m.Report != null && m.Report.DepartmentId == dept.Id)
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => GetMonthNumber(m.Month))
                    .FirstOrDefault();

                var threshold = thresholds.FirstOrDefault(t => t.KPIName == targetKpi);
                var thresholdVal = threshold?.ThresholdValue ?? 0;
                var currentVal = latestMetric?.Value ?? 0;
                var status = CalculateKpiStatus(targetKpi, currentVal, thresholdVal);

                summaries.Add(new DepartmentSummaryDto
                {
                    DepartmentId = dept.Id,
                    DepartmentName = dept.Name,
                    KPIName = targetKpi,
                    CurrentValue = currentVal,
                    Threshold = thresholdVal,
                    Status = status
                });
            }

            return summaries;
        }

        public async Task<List<AlertDto>> GetRecentAlertsAsync()
        {
            var companyId = _currentUserService.CompanyId;
            var role = _currentUserService.Role;
            var deptId = _currentUserService.DepartmentId;

            var alertsQuery = _dbContext.Alerts
                .Include(a => a.Department)
                .Where(a => a.CompanyId == companyId && !a.SeenByOwner);

            if (role == "Manager" && deptId.HasValue)
            {
                alertsQuery = alertsQuery.Where(a => a.DepartmentId == deptId.Value);
            }

            return await alertsQuery
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new AlertDto
                {
                    Id = a.Id,
                    KPIName = a.KPIName,
                    CurrentValue = a.CurrentValue,
                    Threshold = a.Threshold,
                    Message = a.Message,
                    Recommendation = a.Recommendation,
                    SeenByOwner = a.SeenByOwner,
                    CreatedAt = a.CreatedAt,
                    DepartmentName = a.Department != null ? a.Department.Name : "General"
                })
                .ToListAsync();
        }
    }
}
