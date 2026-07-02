using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Persistence.Repositories
{
    public class MetricsRepository : IMetricsRepository
    {
        private readonly AppDbContext _context;

        public MetricsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<KPIConfig?> GetKPIConfigAsync(int companyId, string kpiName)
        {
            return await _context.KPIs
                .Where(k => k.CompanyId == companyId && k.Name == kpiName)
                .Select(k => new KPIConfig
                {
                    Threshold = k.Threshold,
                    AlertPercentageDiff = k.AlertPercentageDiff,
                    TrendMonthsCount = k.TrendMonthsCount
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<decimal>> GetLastNMonthsAsync(int companyId, string kpiName, int n)
        {
            return await _context.HistoricalMetrics
                .Where(m => m.CompanyId == companyId
                && m.KPIName == kpiName)
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .Take(n)
                .Select(m => m.Value)
                .ToListAsync();
        }

        public async Task<decimal?> GetSameMonthLastYearAsync(int companyId, string kpiName, int month)
        {
            var lastYear = DateTime.UtcNow.Year - 1;
            return await _context.HistoricalMetrics
                .Where(m => m.CompanyId == companyId
                && m.KPIName == kpiName
                && m.Month == month
                && m.Year == lastYear)
                .Select(m => (decimal?)m.Value)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> GetThresholdAsync(int companyId, string kpiName)
        {
            return await _context.KPIs
                .Where(k => k.CompanyId == companyId && k.Name == kpiName)
                .Select(k => (decimal?)k.Threshold)
                .FirstOrDefaultAsync();
        }
    }
}
