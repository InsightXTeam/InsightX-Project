using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;
using InsightX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Repositories
{
    public class ExtractedMetricRepository : IExtractedMetricRepository
    {
        private readonly AppDbContext _context;

        public ExtractedMetricRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExtractedMetric>> GetByReportIdAsync(int reportId)
        {
            return await _context.ExtractedMetrics
                .Where(x => x.ReportId == reportId)
                .ToListAsync();
        }

        public async Task AddRangeAsync(List<ExtractedMetric> metrics)
        {
            await _context.ExtractedMetrics.AddRangeAsync(metrics);

            await _context.SaveChangesAsync();
        }
    }
}