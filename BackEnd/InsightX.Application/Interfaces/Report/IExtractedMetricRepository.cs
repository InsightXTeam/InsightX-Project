using InsightX.Domain.Entities.Reports;

namespace InsightX.Application.Interfaces
{
    public interface IExtractedMetricRepository
    {
        Task<List<ExtractedMetric>> GetByReportIdAsync(int reportId);

        Task AddRangeAsync(List<ExtractedMetric> metrics);
    }
}